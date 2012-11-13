using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using TinyIoC;
using ServiceDiscovery;

namespace IntAirAct
{
    public delegate void DeviceFoundHandler(IADevice device, bool ownDevice);
    public delegate void DeviceLostHandler(IADevice device);

    public class IAIntAirAct : IDisposable
    {
        public HashSet<IARoute> SupportedRoutes { get; private set; }
        public List<IADevice> devices { get; private set; }
        public bool isRunning { get; private set; }
        public IADevice ownDevice { get; private set; }
        public event DeviceFoundHandler DeviceFound;
        public event DeviceLostHandler DeviceLost;

        public ushort port
        {
            get
            {
                return this.server.Port;
            }

            set
            {
                this.server.Port = value;
            }
        }

        public string type { get; set; }

        private bool isDisposed = false;
        private Dictionary<string, Type> mappings = new Dictionary<string, Type>();
        private IAServer server;
        private SDServiceDiscovery serviceDiscovery;

        public IAIntAirAct(IAServer server, SDServiceDiscovery serviceDiscovery)
        {
            this.server = server;
            this.serviceDiscovery = serviceDiscovery;
            SupportedRoutes = new HashSet<IARoute>();
            isRunning = false;
            port = 0;

            this.Setup();
        }

        ~IAIntAirAct()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                // Code to dispose the managed resources of the class
                Stop();
                if (serviceDiscovery != null)
                {
                    serviceDiscovery.Dispose();
                }
            }
            // Code to dispose the un-managed resources of the class
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            if (isRunning)
            {
                return;
            }

            server.Start();

            serviceDiscovery.publishService("_intairact._tcp.", port);
            serviceDiscovery.SearchForServices("_intairact._tcp.");

            isRunning = true;
        }

        public void Stop()
        {
            if (serviceDiscovery != null)
            {
                serviceDiscovery.Stop();
            }

            if (server != null)
            {
                server.Stop();
            }

            isRunning = false;
        }

        public Object DeserializeObject(JObject token)
        {
            foreach (KeyValuePair<string, JToken> keyvaluepair in token)
            {
                Type t;
                if (mappings.TryGetValue(keyvaluepair.Key, out t))
                {
                    JsonSerializer ser = new JsonSerializer();
                    using (JTokenReader jsonReader = new JTokenReader(keyvaluepair.Value))
                    {
                        return ser.Deserialize(jsonReader, t);
                    }
                }
            }
            return null;
        }

        public void AddMappingForClass(Type type, string rootKeypath)
        {
            mappings.Add(rootKeypath, type);
        }

        public void CallAction(IAAction action, IADevice device)
        {
            RestClient client = new RestClient(String.Format("http://{0}:{1}", device.Host, device.Port));
            RestRequest request = new RestRequest("action/{action}", Method.PUT);
            request.AddUrlSegment("action", action.action);
            string json = "{'actions':" + JsonConvert.SerializeObject(action) + "}";
            Console.WriteLine("Sending an action: " + json);
            request.AddBody(json);
            client.Execute(request);
        }

        public void Route(IARoute route, Action<IARequest, IAResponse> action)
        {
            SupportedRoutes.Add(route);
            server.Route(route, action);
        }

        private void Setup()
        {
            AddMappingForClass(typeof(IADevice), "devices");
            AddMappingForClass(typeof(IAAction), "actions");
            AddMappingForClass(typeof(IARoute), "capabilities");

            serviceDiscovery.ServiceFound += new ServiceFoundHandler(this.OnServiceFound);
            serviceDiscovery.ServiceLost += new ServiceLostHandler(this.OnServiceLost);
            serviceDiscovery.ServiceDiscoveryError += new ServiceDiscoveryErrorHandler(this.OnServiceDiscoveryError);

            this.Route(new IARoute("GET", "/routes"), delegate(IARequest request, IAResponse response)
            {
                response.RespondWith(this.SupportedRoutes, "routes");
            });
        }

        private void OnServiceFound(SDService service, bool ownService)
        {
            if (ownService)
            {
                IADevice device = new IADevice(service.Name, service.Hostname, service.Port, this.SupportedRoutes);
                this.ownDevice = device;
                OnDeviceFound(device, true);
            }
            else
            {
                IADevice device = new IADevice(service.Name, service.Hostname, service.Port, null);
                this.devices.Add(device);
                OnDeviceFound(device, false);
            }
        }

        private void OnServiceLost(SDService service)
        {
            IADevice device = new IADevice(service.Name, service.Hostname, service.Port, null);
            this.devices.Remove(device);
            OnDeviceLost(device);
        }

        private void OnServiceDiscoveryError(EventArgs eventArg)
        {
            Console.WriteLine(String.Format("An error ocurred: {0}", eventArg));
        }

        protected virtual void OnDeviceFound(IADevice device, bool ownDevice)
        {
            if (DeviceFound != null)
            {
                DeviceFound(device, ownDevice);
            }
        }

        protected virtual void OnDeviceLost(IADevice device)
        {
            if (DeviceLost != null)
            {
                DeviceLost(device);
            }
        }
    }
}
