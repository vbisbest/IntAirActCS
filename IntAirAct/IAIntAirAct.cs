using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Net.NetworkInformation;
using ZeroConf;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace IntAirAct
{
    public delegate void DeviceUpdateEventHandler(object sender, EventArgs e);

    public class IAIntAirAct : IDisposable
    {
        public HashSet<IACapability> capabilities { get; private set; }
        public bool client { get; set; }
        public string defaultMimeType { get; set; }
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
        public bool server { get; set; }
        public string type { get; set; }

        private ZCZeroConf zeroConf = new ZCZeroConf();
        private bool isDisposed = false;
        private Dictionary<string, Type> mappings = new Dictionary<string, Type>();

        public IAIntAirAct()
        {
            capabilities = new HashSet<IACapability>();
            client = true;
            defaultMimeType = "application/json";
            isRunning = false;
            port = 0;
            server = true;

            AddMappingForClass(typeof(IADevice), "devices");
            AddMappingForClass(typeof(IAAction), "actions");
            AddMappingForClass(typeof(IACapability), "capabilities");
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

            if (server)
            {
                if (port == 0)
                {
                    // find next free port
                    port = TcpPort.FindNextAvailablePort(12345);
                }

                new Gate.Hosts.Firefly.ServerFactory().Create(Gate.Adapters.Nancy.NancyAdapter.App(), port);
            }

            try
            {
                zeroConf.serviceUpdateEventHandler += new ServiceUpdateEventHandler(ServiceUpdate);
                zeroConf.publishRegType = "_intairact._tcp";
                zeroConf.publishPort = port;
                zeroConf.server = server;
                zeroConf.Start();
            }
            catch (ZeroConfException e)
            {
                throw new IntAirActException(e.Message);
            }

            isRunning = true;
            TinyIoC.TinyIoCContainer.Current.Register<IAIntAirAct>(this);
        }

        public void Stop()
        {
            if (zeroConf != null)
            {
                zeroConf.Stop();
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

        public static class TcpPort
        {
            private const string PortReleaseGuid =
                "8875BD8E-4D5B-11DE-B2F4-691756D89593";

            /// <summary>
            /// Check if startPort is available, incrementing and
            /// checking again if it's in use until a free port is found
            /// </summary>
            /// <param name="startPort">The first port to check</param>
            /// <returns>The first available port</returns>
            public static ushort FindNextAvailablePort(ushort startPort)
            {
                ushort port = startPort;
                bool isAvailable = true;

                var mutex = new Mutex(false,
                    string.Concat("Global/", PortReleaseGuid));
                mutex.WaitOne();
                try
                {
                    IPGlobalProperties ipGlobalProperties =
                        IPGlobalProperties.GetIPGlobalProperties();
                    IPEndPoint[] endPoints =
                        ipGlobalProperties.GetActiveTcpListeners();

                    do
                    {
                        if (!isAvailable)
                        {
                            port++;
                            isAvailable = true;
                        }

                        foreach (IPEndPoint endPoint in endPoints)
                        {
                            if (endPoint.Port != port) continue;
                            isAvailable = false;
                            break;
                        }

                    } while (!isAvailable && port < IPEndPoint.MaxPort);

                    if (!isAvailable)
                        throw new Exception();

                    return port;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
