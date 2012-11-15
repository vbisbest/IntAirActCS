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
            
            // don't mess with the order here, TinyIoC is very picky about it
            TinyIoCContainer container = TinyIoCContainer.Current;
            NancyServerAdapter adapter = new NancyServerAdapter();
            Owin.AppDelegate app = Gate.Adapters.Nancy.NancyAdapter.App();
            adapter.App = app;
            // register the server adapter for the module serving the routes
            container.Register<NancyServerAdapter>(adapter);
            SDServiceDiscovery serviceDiscovery = new SDServiceDiscovery();
            serviceDiscovery.InvokeableObject = form;

            IAIntAirAct ia = new IAIntAirAct(adapter, serviceDiscovery);
           
            // necessary for old module and action handling
            container.Register<IAIntAirAct>(ia);
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
