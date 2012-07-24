using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class IACapability
    {
        public string capability { get; set; }

        public IACapability(string capability)
        {
            this.capability = capability;
        }

        public override string ToString()
        {
            return String.Format("Capability[capability: {0}]", capability);
        }
    }
}
