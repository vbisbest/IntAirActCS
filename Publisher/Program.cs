using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using ServiceDiscovery;

namespace Publisher
{
    static class Program
    {
        private static TraceSource logger = new TraceSource("Publisher");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form form1 = new Form1();
            logger.TraceEvent(TraceEventType.Start, 0);

            SDServiceDiscovery serviceDiscovery;
            try
            {
                serviceDiscovery = new SDServiceDiscovery();
                serviceDiscovery.InvokeableObject = form1;
                serviceDiscovery.SearchForServices("_intairact._tcp.");
                Application.Run(form1);
                serviceDiscovery.Stop();
            } catch (Exception e)
            {
                logger.TraceEvent(TraceEventType.Critical, 0, e.Message);
            }

            logger.TraceEvent(TraceEventType.Stop, 0);
        }
    }
}
