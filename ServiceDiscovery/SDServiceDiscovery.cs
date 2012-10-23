using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ServiceDiscovery
{
    public class SDServiceDiscovery
    {
        private static TraceSource logger = new TraceSource("SDServiceDiscovery");

        private bool isDisposed = false;

        ~SDServiceDiscovery()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                // Code to dispose the managed resources of the class
                stop();
            }
            // Code to dispose the un-managed resources of the class
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
            
        public void stop()
        {
            stopSearching();
        }

        public void stopSearching()
        {
            logger.TraceEvent(TraceEventType.Verbose, 0, "stopSearching");
        }

        public void searchForServices()
        {
        }
    }
}
