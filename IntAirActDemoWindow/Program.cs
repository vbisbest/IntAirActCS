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

            Capability ca = new Capability();
            ca.capability = "PUT /action/displayImage";
            ia.capabilities.Add(ca);

            Capability ca2 = new Capability();
            ca2.capability = "GET /images";
            ia.capabilities.Add(ca2);

            ia.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            ia.Stop();
        }
    }
}
