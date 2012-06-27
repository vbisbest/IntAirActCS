using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace IntAirAct
{
    public class CapabilitiesModule : NancyModule
    {
        public CapabilitiesModule(IAIntAirAct intAirAct)
        {
            Get["/capabilities"] = parameters => Response.RespondWith(intAirAct.capabilities, "capabilities");
        }

    }
}
