using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ZeroconfService;

namespace ServiceDiscovery
{
    public class SDServiceDiscovery
    {
        private static TraceSource logger = new TraceSource("ServiceDiscovery");

        public bool IsSearching { get; private set; }

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
            
            netServiceBrowser.SearchForRegistrationDomains();
            
            netServiceBrowser.SearchForService(type, domain);

            netServiceBrowsers[key] = netServiceBrowser;

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
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didFindService: {1}", aNetServiceBrowser, netService));
            netService.DidUpdateTXT += new NetService.ServiceTXTUpdated(netServiceDidUpdateTXTRecordData);
            netService.DidResolveService += new NetService.ServiceResolved(netServiceDidResolveAddress);
            netService.DidNotResolveService += new NetService.ServiceNotResolved(netServiceDidNotResolve);
            netService.ResolveWithTimeout(10);
        }

        void netServiceBrowserDidRemoveService(NetServiceBrowser aNetServiceBrowser, NetService netService, bool moreComing)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didRemoveService: {1}", aNetServiceBrowser, netService));
        }

        void netServiceDidNotResolve(NetService sender, DNSServiceException exception)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didNotResolve: {1}", sender, exception));
        }

        void netServiceDidResolveAddress(NetService sender)
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, String.Format("{0}: didResolveAddress", sender));
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
            //TODO: Send out Notification of resolved Service
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

        private String keyForSearch(String type, String domain)
        {
            return String.Format("{0}{1}", type, domain);
        }

    }
}