using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using TinyIoC;
using ServiceDiscovery;
using System.Net;
using System.ComponentModel;

namespace IntAirAct
{
    public delegate void DeviceFoundHandler(IADevice device, bool ownDevice);
    public delegate void DeviceLostHandler(IADevice device);

    public class IAIntAirAct : IDisposable
    {
        public bool IsRunning { get; private set; }
        public IADevice OwnDevice { get; private set; }
        public HashSet<IARoute> SupportedRoutes { get; set; }
        public event DeviceFoundHandler DeviceFound;
        public event DeviceLostHandler DeviceLost;

        private bool isDisposed = false;
        private Dictionary<string, Type> mappings = new Dictionary<string, Type>();
        private IAServer server;
        private SDServiceDiscovery serviceDiscovery;
        private List<IADevice> devices = new List<IADevice>();

        #region Constructor, Deconstructor

        public static IAIntAirAct Instance(ISynchronizeInvoke invokableObject)
        {
            // don't mess with the order here, TinyIoC is very picky about it
            TinyIoCContainer container = TinyIoCContainer.Current;
            NancyServerAdapter adapter = new NancyServerAdapter();
            Owin.AppDelegate app = Gate.Adapters.Nancy.NancyAdapter.App();
            adapter.App = app;
            // register the server adapter for the module serving the routes
            container.Register<NancyServerAdapter>(adapter);
            SDServiceDiscovery serviceDiscovery = new SDServiceDiscovery();

            return new IAIntAirAct(adapter, serviceDiscovery);
        }

        public IAIntAirAct(IAServer server, SDServiceDiscovery serviceDiscovery)
        {
            this.server = server;
            this.serviceDiscovery = serviceDiscovery;
            this.IsRunning = false;
            this.SupportedRoutes = new HashSet<IARoute>();
            Port = 0;

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

        #endregion
        #region Start, Stop, Setup

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            server.Start();

            serviceDiscovery.PublishService("_intairact._tcp.", Port);
            serviceDiscovery.SearchForServices("_intairact._tcp.");

            IsRunning = true;
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

            IsRunning = false;
        }

        private void Setup()
        {
            AddMappingForClass(typeof(IADevice), "devices");
            AddMappingForClass(typeof(IAAction), "actions");
            AddMappingForClass(typeof(IARoute), "routes");

            serviceDiscovery.ServiceFound += new ServiceFoundHandler(this.OnServiceFound);
            serviceDiscovery.ServiceLost += new ServiceLostHandler(this.OnServiceLost);
            serviceDiscovery.ServiceDiscoveryError += new ServiceDiscoveryErrorHandler(this.OnServiceDiscoveryError);

            this.Route(new IARoute("GET", "/routes"), delegate(IARequest request, IAResponse response)
            {
                response.RespondWith(this.SupportedRoutes, "routes");
            });
        }

        #endregion
        #region Properties

        public ushort Port
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

        public List<IADevice> Devices
        {
            get
            {
                return new List<IADevice>(this.devices);
            }
        }

        #endregion
        #region Methods

        public IEnumerable<IADevice> DevicesSupportingRoute(IARoute route)
        {
            return this.Devices.FindAll(device => device.SupportedRoutes.Contains(route));
        }

        public void Route(IARoute route, Action<IARequest, IAResponse> action)
        {
            SupportedRoutes.Add(route);
            server.Route(route, action);
        }

        #endregion
        #region Event Handling

        private void OnServiceFound(SDService service, bool ownService)
        {
            if (ownService)
            {
                IADevice device = new IADevice(service.Name, service.Hostname, service.Port, this.SupportedRoutes);
                this.OwnDevice = device;
                OnDeviceFound(device, true);
            }
            else
            {
                RestClient client = new RestClient(String.Format("http://{0}:{1}", service.Hostname, service.Port));
                RestRequest request = new RestRequest("/routes", Method.GET);
                client.ExecuteAsync(request, response =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine(response.Content);
                        IADevice device = new IADevice(service.Name, service.Hostname, service.Port, null);
                        this.devices.Add(device);
                        OnDeviceFound(device, false);
                    }
                    else
                    {
                        Console.WriteLine(String.Format("An error ocurred trying to request routes from {0}", client.BaseUrl));
                    }
                });
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

        #endregion
        #region

        [Obsolete("This method is obsolete")]
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

        [Obsolete("This method is obsolete")]
        public void AddMappingForClass(Type type, string rootKeypath)
        {
            mappings.Add(rootKeypath, type);
        }

        [Obsolete("This method is obsolete")]
        public void CallAction(IAAction action, IADevice device)
        {
            RestClient client = new RestClient(String.Format("http://{0}:{1}", device.Host, device.Port));
            RestRequest request = new RestRequest("action/{action}", Method.PUT);
            request.AddUrlSegment("action", action.action);
            string json = "{\"actions\":" + JsonConvert.SerializeObject(action) + "}";
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            client.ExecuteAsync(request, response =>
            {
                Console.WriteLine("CallAction response: " + response.Content);
            });
        }

        #endregion
    }
}
