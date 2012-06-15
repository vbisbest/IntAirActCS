using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class Hello
    {
        private int i = 0;

        public string hello()
        {
            i++;
            return "Hello World for the " + i + " time";
        }
    }
}
