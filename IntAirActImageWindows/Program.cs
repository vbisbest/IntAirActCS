using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using IntAirAct;

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

            IAIntAirAct ia = new IAIntAirAct();
            ia.client = false;
            ia.AddMappingForClass(typeof(Image), "images");
            //ia.RouteClass(typeof(Image), "/images/:identifier");
            ia.capabilities.Add(new IACapability("PUT /action/displayImage"));
            ia.Start();
            
            Application.Run(form);

            ia.Stop();
        }
    }
}