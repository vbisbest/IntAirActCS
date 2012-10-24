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
            logger.TraceEvent(TraceEventType.Start, 0);

            SDServiceDiscovery serviceDiscovery;
            try
            {
                serviceDiscovery = new SDServiceDiscovery();
                serviceDiscovery.Stop();
            } catch (Exception e)
            {
                logger.TraceEvent(TraceEventType.Critical, 0, e.Message);
            }
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            logger.TraceEvent(TraceEventType.Stop, 0);
        }
    }
}
