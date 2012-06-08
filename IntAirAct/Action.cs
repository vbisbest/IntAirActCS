using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class Action
    {
        public string action { get; set; }
        public Array parameters { get; set; }

        public override string ToString()
        {
            return String.Format("Action[action: {0}, parameters: {1}]", action, parameters);
        }
    }
}
