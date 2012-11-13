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
            stopPublishing();
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

            String key = this.keyForSearch(type, domain);

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
            netServiceBrowser.InvokeableObject = this.InvokeableObject;
            netServiceBrowser.DidFindDomain += new NetServiceBrowser.DomainFound(netServiceBrowserDidFindDomain);
            netServiceBrowser.DidRemoveDomain += new NetServiceBrowser.DomainRemoved(netServiceBrowserDidRemoveDomain);
            netServiceBrowser.DidFindService += new NetServiceBrowser.ServiceFound(netServiceBrowserDidFindService);
            netServiceBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(netServiceBrowserDidRemoveService);

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
            String key = this.keyForSearch(type, domain);

            if (netServiceBrowsers.ContainsKey(key))
            {
                netServiceBrowsers[key].Stop();
                netServiceBrowsers.Remove(key);
            }

            this.IsSearching = (netServiceBrowsers.Count != 0);
        }

        public bool publishService(String type, ushort port)
        {
            return this.publishService(type, port, "");
        }

        public bool publishService(String type, ushort port, String name)
        {
            return this.publishService(type, port, name, "");
        }

        public bool publishService(String type, ushort port, String name, String domain)
        {
            return this.publishService(type, port, name, domain, new Dictionary<string,string>());
        }

        public bool publishService(String type, ushort port, String name, String domain, Dictionary<String, String> TXTRecord)
        {
            if (!type.EndsWith("."))
            {
                type += ".";
            }

            String key = this.keyForPublish(type, domain, port);

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

            netService.DidPublishService += new NetService.ServicePublished(netServiceDidPublish);
            netService.DidNotPublishService += new NetService.ServiceNotPublished(netServiceDidNotPublish);

            /* HARDCODE TXT RECORD */
            System.Collections.Hashtable dict = new System.Collections.Hashtable();
            dict.Add("txtvers", "1");
            netService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dict);

            netServices[key] = netService;

            netService.Publish();

            logger.TraceEvent(TraceEventType.Information, 0, String.Format("Published service of type {0} in domain {1} on port {2}", type, domain, port));

            return true;
        }

        void stopPublishing()
        {
            foreach (NetService netService in netServices.Values)
            {
                netService.Stop();
            }
            netServices.Clear();

            this.IsPublishing = false;
        }

        void stopPublishingService(String type, ushort port)
        {
            this.stopPublishingService(type, port, "");
        }

        void stopPublishingService(String type, ushort port, String domain)
        {
            String key = this.keyForPublish(type, domain, port);

            if (netServices.ContainsKey(key))
            {
                netServices[key].Stop();
                netServices.Remove(key);
            }

            this.IsPublishing = (netServices.Count != 0);
        }

        void netServiceBrowserDidFindDomain(NetServiceBrowser aNetServiceBrowser, string domainString, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didFindDomain: {1}", aNetServiceBrowser, domainString));
        }

        void netServiceBrowserDidRemoveDomain(NetServiceBrowser aNetServiceBrowser, string domainString, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didRemoveDomain: {1}", aNetServiceBrowser, domainString));
        }

        void netServiceBrowserDidFindService(NetServiceBrowser aNetServiceBrowser, NetService netService, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Information, 0, String.Format("{0}: didFindService: {1}", aNetServiceBrowser, netService));
            netService.DidUpdateTXT += new NetService.ServiceTXTUpdated(netServiceDidUpdateTXTRecordData);
            netService.DidResolveService += new NetService.ServiceResolved(netServiceDidResolveAddress);
            netService.DidNotResolveService += new NetService.ServiceNotResolved(netServiceDidNotResolve);
            netService.ResolveWithTimeout(10);
        }

        void netServiceBrowserDidRemoveService(NetServiceBrowser aNetServiceBrowser, NetService netService, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didRemoveService: {1}", aNetServiceBrowser, netService));
        }

        void netServiceDidPublish(NetService sender)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didPublish", sender));
        }

        void netServiceDidNotPublish(NetService sender, DNSServiceException exception)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didNotPublish: {1}", sender, exception));
        }

        void netServiceDidNotResolve(NetService sender, DNSServiceException exception)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didNotResolve: {1}", sender, exception));
        }

        void netServiceDidResolveAddress(NetService sender)
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

        void netServiceDidUpdateTXTRecordData(NetService sender)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didUpdateTXTRecordData", sender));
        }

        System.ComponentModel.ISynchronizeInvoke mInvokeableObject = null;
        public System.ComponentModel.ISynchronizeInvoke InvokeableObject
        {
            get { return mInvokeableObject; }
            set { mInvokeableObject = value; }
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

        private String keyForSearch(String type, String domain)
        {
            return String.Format("{0}{1}", type, domain);
        }

        private String keyForPublish(String type, String domain, ushort port)
        {
            return String.Format("{0}{1}:{2}", type, domain, port);
        }

    }
}