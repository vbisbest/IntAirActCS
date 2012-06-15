using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Nancy.Hosting.Self;
using System.Threading;
using System.Net.NetworkInformation;
using ZeroConf;

namespace IntAirAct
{
    public delegate void DeviceUpdateEventHandler(object sender, EventArgs e);

    public class IAIntAirAct : IDisposable
    {
        private int i = 0;

        public HashSet<Capability> capabilities { get; private set; }
        public bool client { get; set; }
        public string defaultMimeType { get; set; }
        public List<Device> devices { get; private set; }
        public event ServiceUpdateEventHandler deviceUpdateEventHandler;
        public bool isRunning { get; private set; }
        public Device ownDevice
        {
            get
            {
                Service service = zeroConf.ownService;
                return new Device(service.name, service.host, service.port);
            }
            private set
            {
            }
        }
        public ushort port { get; set; }
        public bool server { get; set; }
        public string type { get; set; }

        private NancyHost host;
        private ZCZeroConf zeroConf;
        private bool isDisposed = false;

        public IAIntAirAct()
        {
            capabilities = new HashSet<Capability>();
            client = true;
            defaultMimeType = "application/json";
            isRunning = false;
            port = 0;
            server = true;
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

                try
                {
                    host = new NancyHost(GetUriParams(port));
                    host.Start();
                }
                catch (HttpListenerException e)
                {
                    throw new IntAirActException(e.Message);
                }
            }

            try
            {
                zeroConf = new ZCZeroConf();
                zeroConf.serviceUpdateEventHandler += new ServiceUpdateEventHandler(serviceUpdate);
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
            if (host != null)
            {
                host.Stop();
            }

            isRunning = false;
        }

        private void notifyDeviceUpdate()
        {
            if (deviceUpdateEventHandler != null)
                deviceUpdateEventHandler(this, EventArgs.Empty);
        }

        private void serviceUpdate(object sender, EventArgs e)
        {
            List<Device> list = new List<Device>();

            // synchronize this.devices with zeroConf.devices
            foreach (Service service in zeroConf.services)
            {
                Device d = new Device(service.name, service.host, service.port);
                list.Add(d);
            }

            devices = list;

            notifyDeviceUpdate();
        }

        private static Uri[] GetUriParams(ushort port)
        {
            var uriParams = new List<Uri>();
            string hostName = Dns.GetHostName();
            // Host name URI
            string hostNameUri = string.Format("http://{0}:{1}", Dns.GetHostName(), port);
            uriParams.Add(new Uri(hostNameUri));
            // Host address URI(s) 
            var hostEntry = Dns.GetHostEntry(hostName);
            foreach (var ipAddress in hostEntry.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)  // IPv4 addresses only 
                {
                    var addrBytes = ipAddress.GetAddressBytes();
                    string hostAddressUri = string.Format("http://{0}.{1}.{2}.{3}:{4}", addrBytes[0], addrBytes[1], addrBytes[2], addrBytes[3], port);
                    uriParams.Add(new Uri(hostAddressUri));
                }
            }
            // Localhost URI 
            uriParams.Add(new Uri(string.Format("http://localhost:{0}", port)));
            return uriParams.ToArray();
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
