using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Bonjour;

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
                new DNSSDService();
            }
            catch
            {
                logger.TraceEvent(TraceEventType.Critical, 0, "Bonjour Service not available");
                throw new Exception("Bonjour Service not available");
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

        public void StopSearching()
        {
            logger.TraceEvent(TraceEventType.Stop, 0);
        }

        public void SearchForServices()
        {
        }
    }
}
