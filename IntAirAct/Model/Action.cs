using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class Action
    {
        public string action { get; set; }
        public List<Object> parameters { get; set; }

        public Action()
        {
            parameters = new List<Object>();
        }

        public override string ToString()
        {
            return String.Format("Action[action: {0}, parameters: {1}]", action, parameters);
        }
    }
}
