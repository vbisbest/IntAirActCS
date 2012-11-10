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
            netServiceBrowsers[key] = netServiceBrowser;
            netServiceBrowser.SearchForService(type, domain);

            logger.TraceEvent(TraceEventType.Information, 0, String.Format("Search started for type {0} in domain {1}", type, domain));

            return true;
        }

        public void StopSearching()
        {
            logger.TraceEvent(TraceEventType.Stop, 0);

            foreach (NetServiceBrowser netServiceBrowser in netServiceBrowsers.Values)
            {
                netServiceBrowser.Stop();
            }
            netServiceBrowsers.Clear();
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