using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IAIntAirAct;

namespace IntAirActDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IntAirAct ia = new IntAirAct();
            
            ia.start();
            Console.WriteLine("Started IntAirAct");
            Console.ReadLine();
            ia.stop();
        }
    }
}
