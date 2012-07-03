using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using IntAirAct;

namespace IntAirActDemoWindow
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            IAIntAirAct ia = new IAIntAirAct();

            ia.AddMappingForClass(typeof(Image), "images");

            ia.capabilities.Add(new Capability("GET /images"));

            ia.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            ia.Stop();
        }
    }
}
