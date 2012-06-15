using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class Capability
    {
        public string capability { get; set; }

        public override string ToString()
        {
            return String.Format("Capability[capability: {0}]", capability);
        }
    }
}
