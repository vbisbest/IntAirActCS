using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IntAirAct;
using System.Diagnostics;

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
            Debug.WriteLine("Test");

            IAIntAirAct ia = new IAIntAirAct();

            ia.Start();
            Console.WriteLine("Started IntAirAct");
            Console.ReadLine();
            ia.Stop();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
