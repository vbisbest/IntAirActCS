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

        public void SearchForServices(String type)
        {
            SearchForServices(type, "");
        }

        public void SearchForServices(String type, String domain)
        {
        }

        public void StopSearching()
        {
            logger.TraceEvent(TraceEventType.Stop, 0);
        }

        public void SearchForServices()
        {

        }
    }
}