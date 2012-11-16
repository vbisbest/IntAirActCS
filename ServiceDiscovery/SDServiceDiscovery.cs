using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ZeroconfService;

namespace ServiceDiscovery
{
    public delegate void ServiceFoundHandler(SDService service, bool ownService);
    public delegate void ServiceLostHandler(SDService sender);
    public delegate void ServiceDiscoveryErrorHandler(EventArgs e);

    public class SDServiceDiscovery : IDisposable
    {
        private static TraceSource logger = new TraceSource("ServiceDiscovery");

        public bool IsSearching { get; private set; }
        public bool IsPublishing { get; private set; }
        public event ServiceFoundHandler ServiceFound;
        public event ServiceLostHandler ServiceLost;
        public event ServiceDiscoveryErrorHandler ServiceDiscoveryError;

        private Dictionary<String, NetServiceBrowser> netServiceBrowsers;
        private Dictionary<String, NetService> netServices;
        private bool isDisposed = false;

        public SDServiceDiscovery()
        {
            try
            {
                logger.TraceEvent(TraceEventType.Information, 0, String.Format("Bonjour Version: {0}", NetService.DaemonVersion));
            }
            catch (Exception ex)
            {
                String message = ex is DNSServiceException ? "Bonjour is not installed!" : ex.Message;
                logger.TraceEvent(TraceEventType.Critical, 0, message);
                throw new Exception(message);
            }

            netServiceBrowsers = new Dictionary<string, NetServiceBrowser>();
            netServices = new Dictionary<string, NetService>();
        }

        ~SDServiceDiscovery()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                // Code to dispose the managed resources of the class
                Stop();
            }
            // Code to dispose the un-managed resources of the class
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
            
        public void Stop()
        {
            logger.TraceEvent(TraceEventType.Stop, 0);
            StopPublishing();
            StopSearching();
        }

        public bool SearchForServices(String type)
        {
            return SearchForServices(type, "");
        }

        public bool SearchForServices(String type, String domain)
        {
            if (!type.EndsWith("."))
            {
                type += ".";
            }

            String key = this.KeyForSearch(type, domain);

            if (this.IsSearching)
            {
                if (netServiceBrowsers.ContainsKey(key))
                {
                    logger.TraceEvent(TraceEventType.Warning, 0, String.Format("Already searching for type {0} in domain {1}", type, domain));
                    return false;
                }
            }

            this.IsSearching = true;

            NetServiceBrowser netServiceBrowser = new NetServiceBrowser();
            netServiceBrowser.AllowApplicationForms = false;
            netServiceBrowser.AllowMultithreadedCallbacks = true;

            netServiceBrowser.DidFindDomain += new NetServiceBrowser.DomainFound(NetServiceBrowserDidFindDomain);
            netServiceBrowser.DidRemoveDomain += new NetServiceBrowser.DomainRemoved(NetServiceBrowserDidRemoveDomain);
            netServiceBrowser.DidFindService += new NetServiceBrowser.ServiceFound(NetServiceBrowserDidFindService);
            netServiceBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(NetServiceBrowserDidRemoveService);

            netServiceBrowsers[key] = netServiceBrowser;
            
            netServiceBrowser.SearchForService(type, domain);

            logger.TraceEvent(TraceEventType.Information, 0, String.Format("Search started for type {0} in domain {1}", type, domain));

            return true;
        }

        public void StopSearching()
        {
            foreach (NetServiceBrowser netServiceBrowser in netServiceBrowsers.Values)
            {
                netServiceBrowser.Stop();
            }
            netServiceBrowsers.Clear();

            this.IsSearching = false;
        }

        public void StopSearchingForServices(String type)
        {
            this.StopSearchingForServices(type, "");
        }

        public void StopSearchingForServices(String type, String domain)
        {
            String key = this.KeyForSearch(type, domain);

            if (netServiceBrowsers.ContainsKey(key))
            {
                netServiceBrowsers[key].Stop();
                netServiceBrowsers.Remove(key);
            }

            this.IsSearching = (netServiceBrowsers.Count != 0);
        }

        public bool PublishService(String type, ushort port)
        {
            return this.PublishService(type, port, "");
        }

        public bool PublishService(String type, ushort port, String name)
        {
            return this.PublishService(type, port, name, "");
        }

        public bool PublishService(String type, ushort port, String name, String domain)
        {
            return this.PublishService(type, port, name, domain, new Dictionary<string,string>());
        }

        public bool PublishService(String type, ushort port, String name, String domain, Dictionary<String, String> TXTRecord)
        {
            if (!type.EndsWith("."))
            {
                type += ".";
            }

            String key = this.KeyForPublish(type, domain, port);

            if (this.IsPublishing)
            {
                if (netServices.ContainsKey(key))
                {
                    logger.TraceEvent(TraceEventType.Warning, 0, String.Format("Already publishing service of type {0} in domain {1} on port {2}", type, domain, port));
                    return false;
                }
            }

            this.IsPublishing = true;

            NetService netService = new NetService(domain, type, name, port);
            netService.AllowApplicationForms = false;
            netService.AllowMultithreadedCallbacks = true;

            netService.DidPublishService += new NetService.ServicePublished(NetServiceDidPublish);
            netService.DidNotPublishService += new NetService.ServiceNotPublished(NetServiceDidNotPublish);

            /* HARDCODE TXT RECORD */
            System.Collections.Hashtable dict = new System.Collections.Hashtable();
            dict.Add("txtvers", "1");
            netService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dict);

            netServices[key] = netService;

            netService.Publish();

            logger.TraceEvent(TraceEventType.Information, 0, String.Format("Published service of type {0} in domain {1} on port {2}", type, domain, port));

            return true;
        }

        public void StopPublishing()
        {
            foreach (NetService netService in netServices.Values)
            {
                netService.Stop();
            }
            netServices.Clear();

            this.IsPublishing = false;
        }

        public void StopPublishingService(String type, ushort port)
        {
            this.StopPublishingService(type, port, "");
        }

        public void StopPublishingService(String type, ushort port, String domain)
        {
            String key = this.KeyForPublish(type, domain, port);

            if (netServices.ContainsKey(key))
            {
                netServices[key].Stop();
                netServices.Remove(key);
            }

            this.IsPublishing = (netServices.Count != 0);
        }

        protected void NetServiceBrowserDidFindDomain(NetServiceBrowser aNetServiceBrowser, string domainString, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didFindDomain: {1}", aNetServiceBrowser, domainString));
        }

        protected void NetServiceBrowserDidRemoveDomain(NetServiceBrowser aNetServiceBrowser, string domainString, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didRemoveDomain: {1}", aNetServiceBrowser, domainString));
        }

        protected void NetServiceBrowserDidFindService(NetServiceBrowser aNetServiceBrowser, NetService netService, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Information, 0, String.Format("{0}: didFindService: {1}", aNetServiceBrowser, netService));
            netService.DidUpdateTXT += new NetService.ServiceTXTUpdated(NetServiceDidUpdateTXTRecordData);
            netService.DidResolveService += new NetService.ServiceResolved(NetServiceDidResolveAddress);
            netService.DidNotResolveService += new NetService.ServiceNotResolved(NetServiceDidNotResolve);
            netService.ResolveWithTimeout(10);
        }

        protected void NetServiceBrowserDidRemoveService(NetServiceBrowser aNetServiceBrowser, NetService netService, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didRemoveService: {1}", aNetServiceBrowser, netService));
            SDService service = new SDService(netService.Name, netService.HostName, (ushort)netService.Port, netService.Type, null);
            OnServiceLost(service);
        }

        protected void NetServiceDidPublish(NetService sender)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didPublish", sender));
        }

        protected void NetServiceDidNotPublish(NetService sender, DNSServiceException exception)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didNotPublish: {1}", sender, exception));
        }

        protected void NetServiceDidNotResolve(NetService sender, DNSServiceException exception)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didNotResolve: {1}", sender, exception));
        }

        protected void NetServiceDidResolveAddress(NetService sender)
        {
            logger.TraceEvent(TraceEventType.Information, 0, String.Format("{0}: didResolveAddress", sender));
            SDService service = new SDService(sender.Name, sender.HostName, (ushort) sender.Port, sender.Type, null);
            bool ownService = false;
            foreach (NetService netService in this.netServices.Values)
            {
                if (netService.Name.Equals(sender.Name))
                {
                    ownService = true;
                    break;
                }
            }
            OnServiceFound(service, ownService);
        }

        protected void NetServiceDidUpdateTXTRecordData(NetService sender)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didUpdateTXTRecordData", sender));
        }

        protected virtual void OnServiceFound(SDService service, bool ownService)
        {
            if (ServiceFound != null)
            {
                ServiceFound(service, ownService);
            }
        }

        protected virtual void OnServiceLost(SDService service)
        {
            if (ServiceLost != null)
            {
                ServiceLost(service);
            }
        }

        protected virtual void OnServiceDiscoveryError(EventArgs eventArgs)
        {
            if (ServiceDiscoveryError != null)
            {
                ServiceDiscoveryError(eventArgs);
            }
        }

        private String KeyForSearch(String type, String domain)
        {
            return String.Format("{0}{1}", type, domain);
        }

        private String KeyForPublish(String type, String domain, ushort port)
        {
            return String.Format("{0}{1}:{2}", type, domain, port);
        }
    }
}