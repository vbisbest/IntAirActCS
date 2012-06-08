using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;

using Bonjour;

namespace ZeroConf
{
    public delegate void ServiceUpdateEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Zero Configuration using Apple's Bonjour SDK
    /// By Daniel Sabourin
    /// University of Calgary
    /// </summary>
    public class ZCZeroConf : IDisposable
    {
        private DNSSDEventManager m_eventManager = null;
        private DNSSDService m_service = null;

        private DNSSDService m_registrar = null;

        private DNSSDService m_browser = null;
        private DNSSDService m_resolver = null;

        public Service ownService { get; private set; }

        public List<Service> services
        {
            get
            {
                return new List<Service>(resolvedDevices.Values);
            }
            private set
            {
            }
        }

        /// <summary>
        /// When this is true. It will act as a server and publish and browse.
        /// </summary>
        public bool server = true;

        /// <summary>
        /// The serviceName that ZeroConf will publish under. By default it is the system's username
        /// </summary>
        public string publishName = System.Environment.UserName+"'s Computer"; // Default publish name is the system username

        /// <summary>
        /// The regType that ZeroConf will publish under and will browse for. By default it is "_http._tcp"
        /// </summary>
        public string publishRegType = "_http._tcp";

        /// <summary>
        /// The port that ZeroConf will publish under. By default it is 80
        /// </summary>
        public ushort publishPort = 80;

        /// <summary>
        /// This is the name that a service is actually published under. Usually it will be the same as publishName, but if there are name conflicts, it will be different
        /// </summary>
        private string publishedName = "";
        private string publishedFullName = "";

        // There are 2 dictionaries. foundDevices is for all devices that currently have not been resolved (i.e no port)
        // Once a device has been resolved, the entry will move from foundDevices to resolvedDevices and will be removed
        // from foundDevices.
        private static Dictionary<String, Service> foundDevices = new Dictionary<string, Service>();
        private static Dictionary<String, Service> resolvedDevices = new Dictionary<string, Service>();

        public event ServiceUpdateEventHandler serviceUpdateEventHandler;

        /// <summary>
        /// The constructor for Zero Configuration. zc.start() will always start browsing
        /// </summary>
        /// 
        /// <example>
        /// This will browse and publish
        /// <code>
        /// ZeroConf zc = new ZeroConf();
        /// zc.publishName = "Example Name";
        /// zc.publishPort = 80;
        /// zc.publishRegType = "_http._tcp";
        /// zc.server = true;
        /// zc.start();
        /// </code>
        /// 
        /// This will simply browse
        /// <code>
        /// ZeroConf zc = new ZeroConf();
        /// zc.publishRegType = "_http._tcp";
        /// zc.start();
        /// </code>
        /// 
        /// Now, the list of all devices detected can be obtained by calling zc.getDevices()
        /// This will return a List of devices. An event will also triggered whenever a device is found or lost. You must register event handlers if you want to be able to handle these events.
        /// <code>
        /// 
        /// public void FoundDeviceMethod(object sender, EventArgs e) {
        ///     // This method will be called when a device is found and resolved. A good thing to do is to call getDevices() for an updated list.
        /// }
        /// 
        /// public void LostDeviceMethod(object sender, EventArgs e) {
        ///     // This method will be called when a device is lost. A good thing to do is to call getDevices() for an updated list.
        /// }
        /// 
        /// public void registerHandlers() {
        ///     zc.FoundDeviceHandler += new DeviceFoundEventHandler(FoundDeviceMethod);
        ///     zc.LostDeviceHandler += new DeviceLostEventHandler(LostDeviceMethod);
        /// }
        /// public void removeHandlers() {
        ///     zc.FoundDeviceHandler -= new DeviceFoundEventHandler(FoundDeviceMethod);
        ///     zc.LostDeviceHandler -= new DeviceLostEventHandler(LostDeviceMethod);
        /// }
        /// </code>
        /// </example>
        /// 
        public ZCZeroConf()
        {
            try
            {
                m_service = new DNSSDService();
                Console.WriteLine(String.Format("m_service: {0}", m_service.GetHashCode()));
            }
            catch
            {
                throw new ZeroConfException("Bonjour Service not available");
            }

            //
            // Associate event handlers with all the Bonjour events that the app is interested in.
            //
            m_eventManager = new DNSSDEventManager();
            m_eventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(this.ServiceRegistered);
            m_eventManager.ServiceFound += new _IDNSSDEvents_ServiceFoundEventHandler(this.ServiceFound);
            m_eventManager.ServiceLost += new _IDNSSDEvents_ServiceLostEventHandler(this.ServiceLost);
            m_eventManager.ServiceResolved += new _IDNSSDEvents_ServiceResolvedEventHandler(this.ServiceResolved);
            m_eventManager.OperationFailed += new _IDNSSDEvents_OperationFailedEventHandler(this.OperationFailed);
        }

        public void Dispose()
        {
            if (m_registrar != null)
            {
                m_registrar.Stop();
            }

            if (m_browser != null)
            {
                m_browser.Stop();
            }

            if (m_resolver != null)
            {
                m_resolver.Stop();
            }

            m_eventManager.ServiceFound -= new _IDNSSDEvents_ServiceFoundEventHandler(this.ServiceFound);
            m_eventManager.ServiceLost -= new _IDNSSDEvents_ServiceLostEventHandler(this.ServiceLost);
            m_eventManager.ServiceResolved -= new _IDNSSDEvents_ServiceResolvedEventHandler(this.ServiceResolved);
            m_eventManager.OperationFailed -= new _IDNSSDEvents_OperationFailedEventHandler(this.OperationFailed);
        }

        /// <summary>
        /// Starts browsing, and will start publishing if zc.server == true
        /// </summary>
        public void Start()
        {
            try
            {
                if (server)
                {
                    m_registrar = m_service.Register(0, 0, publishName, publishRegType, null, null, publishPort, null, m_eventManager);
                    Console.WriteLine(String.Format("m_registrar: {0}", m_registrar.GetHashCode()));
                }

                m_browser = m_service.Browse(0, 0, publishRegType, null, m_eventManager);
                Console.WriteLine(String.Format("m_browser: {0}", m_browser.GetHashCode()));
            
            }
            catch
            {
                throw new ZeroConfException("Bonjour service is not available");
            }
        }

        /// <summary>
        /// Stops both publishing and browsing, regardless if they were even started.
        /// </summary>
        public void Stop()
        {
            
        }

        private void ServiceRegistered(DNSSDService sref, DNSSDFlags flags, String serviceName, String regType, String domain)
        {
            Console.WriteLine(String.Format("Registered: sref: {0}, flags: {1}, serviceName: {2}, regType: {3}, domain: {4}", sref.GetHashCode(), flags, serviceName, regType, domain));

            publishedName = serviceName;
            publishedFullName = escapeString(serviceName) + "." + regType + "" + domain;
        }

        //
        // ServiceFound
        //
        // Called by DNSServices core as a result of a Browse call
        //
        private void ServiceFound(DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain)
        {
            Console.WriteLine(String.Format("Found: sref: {0}, flags: {1}, ifIndex: {2}, serviceName: {3}, regType: {4}, domain: {5}", sref.GetHashCode(), flags, ifIndex, serviceName, regType, domain));

            String key = escapeString(serviceName) + "." + regType + "" + domain;
            
            if (!foundDevices.ContainsKey(key))
            {
                foundDevices[key] = new Service(serviceName, "", 0);

                DNSSDService res = m_service.Resolve(0, ifIndex, serviceName, regType, domain, m_eventManager);
                Console.WriteLine(String.Format("res: {0}", res.GetHashCode()));
            }
        }

        //
        // ServiceLost
        //
        // Called by DNSServices core as a result of a Browse call
        //
        private void ServiceLost(DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain)
        {
            Console.WriteLine(String.Format("Lost: sref: {0}, flags: {1}, serviceName: {2}, regType: {3}, domain: {4}", sref.GetHashCode(), flags, serviceName, regType, domain));

            // Removes Device from dictionary
            String key = escapeString(serviceName) + "." + regType + "" + domain;
            foundDevices.Remove(key);
            resolvedDevices.Remove(key);

            notifyServiceUpdate();
        }

        //
        // ServiceResolved
        //
        // Called by DNSServices core as a result of DNSService.Resolve()
        // call
        //
        private void ServiceResolved(DNSSDService sref, DNSSDFlags flags, uint ifIndex, String fullName, String hostName, ushort port, TXTRecord txtRecord)
        {
            Console.WriteLine(String.Format("Resolved: sref: {0}, flags: {1}, ifIndex: {2}, fullName: {3}, hostName: {4}, port: {5}, txtRecord: {6}", sref.GetHashCode(), flags, ifIndex, fullName, hostName, port, txtRecord));

            sref.Stop();

            Service service;
            if (foundDevices.TryGetValue(fullName, out service))
            {
                foundDevices.Remove(fullName);
                service.port = port;
                service.host = hostName;

                resolvedDevices[fullName] = service;

                if (fullName.Equals(publishedFullName))
                {
                    ownService = service;
                }

                notifyServiceUpdate();

                Console.WriteLine(service);
            }
            else
            {
                Debug.WriteLine("Cannot find " + fullName + " in dictionary");
            }
        }

        private void OperationFailed(DNSSDService sref, DNSSDError error)
        {
            Console.WriteLine("Operation returned an error code " + error);

            Console.WriteLine(sref.GetHashCode());
        }

        private void notifyServiceUpdate()
        {
            if (serviceUpdateEventHandler != null)
                serviceUpdateEventHandler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Properly escapes a string according to what is valid for a serviceName
        /// </summary>
        /// <param name="s">String to be formatted </param>
        /// <returns>Properly escaped string</returns>
        private static string escapeString(String s)
        {
            return s.Replace("\\", "\\\\").Replace(" ", @"\032").Replace(".", @"\.");
        }
    }
}
