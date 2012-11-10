using System;
using System.Collections.Generic;
using ZeroConf;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using TinyIoC;

namespace IntAirAct
{
    public delegate void DeviceUpdateEventHandler(object sender, EventArgs e);

    public class IAIntAirAct : IDisposable
    {
        public HashSet<IACapability> capabilities { get; private set; }
        public List<IADevice> devices { get; private set; }
        public event ServiceUpdateEventHandler deviceUpdateEventHandler;
        public bool isRunning { get; private set; }
        public IADevice ownDevice
        {
            get
            {
                Service service = zeroConf.ownService;
                return new IADevice(service.name, service.host, service.port);
            }
            private set
            {
            }
        }
        public ushort port { get; set; }
        public string type { get; set; }

        private ZCZeroConf zeroConf = new ZCZeroConf();
        private bool isDisposed = false;
        private Dictionary<string, Type> mappings = new Dictionary<string, Type>();
        private IAServer server;

        public IAIntAirAct(IAServer server)
        {
            capabilities = new HashSet<IACapability>();
            isRunning = false;
            port = 0;
            this.server = server;

            AddMappingForClass(typeof(IADevice), "devices");
            AddMappingForClass(typeof(IAAction), "actions");
            AddMappingForClass(typeof(IACapability), "capabilities");

            this.Route(new IARoute("GET", "/capabilities"), delegate(IARequest request, IAResponse response)
            {
                response.RespondWith(this.capabilities, "capabilities");
            });
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
                if (zeroConf != null)
                {
                    zeroConf.Dispose();
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

            try
            {
                zeroConf.serviceUpdateEventHandler += new ServiceUpdateEventHandler(ServiceUpdate);
                zeroConf.publishRegType = "_intairact._tcp";
                zeroConf.publishPort = port;
                zeroConf.server = true;
                zeroConf.Start();
            }
            catch (ZeroConfException e)
            {
                throw new IntAirActException(e.Message);
            }

            isRunning = true;
        }

        public void Stop()
        {
            if (zeroConf != null)
            {
                zeroConf.Stop();
            }

            if (server != null)
            {
                server.Stop();
            }

            isRunning = false;
        }

        private void ServiceUpdate(object sender, EventArgs e)
        {
            List<IADevice> list = new List<IADevice>();

            // synchronize this.devices with zeroConf.devices
            foreach (Service service in zeroConf.services)
            {
                IADevice d = new IADevice(service.name, service.host, service.port);
                list.Add(d);
            }

            devices = list;

            NotifyDeviceUpdate();
        }

        private void NotifyDeviceUpdate()
        {
            if (deviceUpdateEventHandler != null)
                deviceUpdateEventHandler(this, EventArgs.Empty);
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
            RestClient client = new RestClient(String.Format("http://{0}:{1}", device.host, device.port));
            RestRequest request = new RestRequest("action/{action}", Method.PUT);
            request.AddUrlSegment("action", action.action);
            string json = "{'actions':" + JsonConvert.SerializeObject(action) + "}";
            Console.WriteLine("Sending an action: " + json);
            request.AddBody(json);
            client.Execute(request);
        }

        public void Route(IARoute route, Action<IARequest, IAResponse> action)
        {
            server.Route(route, action);
        }
    }
}
