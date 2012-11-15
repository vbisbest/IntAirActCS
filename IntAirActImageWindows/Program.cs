using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using IntAirAct;
using TinyIoC;
using ServiceDiscovery;

namespace IntAirActImageWindows
{
    static class Program
    {
        private static TraceSource logger = new TraceSource("IntAirActImageWindows");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();

            IAIntAirAct ia = IAIntAirAct.Instance(form);
           
            // necessary for old module and action handling
            TinyIoCContainer.Current.Register<IAIntAirAct>(ia);
            ia.SupportedRoutes.Add(new IARoute("PUT", "/action/displayImage"));
            ia.AddMappingForClass(typeof(Image), "images");
            // end

            ia.Route(new IARoute("PUT", "/views/image"), delegate(IARequest req, IAResponse res)
            {
                logger.TraceEvent(TraceEventType.Information, 0, "Received request on {0}", req.Route);
                
                String url = req.BodyAsString();
                form.LoadImageFromURL(url);
            });
            
            ia.Start();
            
            Application.Run(form);

            ia.Stop();
        }
    }
}
