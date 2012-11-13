using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using IntAirAct;
using TinyIoC;

namespace IntAirActImageWindows
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form form = new Form1();

            // don't mess with the order here, TinyIoC is very picky about it
            TinyIoCContainer container = TinyIoCContainer.Current;
            NancyServerAdapter adapter = new NancyServerAdapter();
            Owin.AppDelegate app = Gate.Adapters.Nancy.NancyAdapter.App();
            adapter.App = app;
            // register the server adapter for the module serving the routes
            container.Register<NancyServerAdapter>(adapter);
            IAIntAirAct ia = new IAIntAirAct(adapter, new ServiceDiscovery.SDServiceDiscovery());

            ia.AddMappingForClass(typeof(Image), "images");
            ia.Start();
            
            Application.Run(form);

            ia.Stop();
        }
    }
}
