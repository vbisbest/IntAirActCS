using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IntAirAct;

namespace IntAirActDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IAIntAirAct ia = new IAIntAirAct();
            
            ia.Start();
            Console.WriteLine("Started IntAirAct");
            Console.ReadLine();
            ia.Stop();
        }
    }
}
