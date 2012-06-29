using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using IntAirAct;

namespace IntAirActDemoWindow
{
    public class DisplayImageAction : NancyModule
    {
        public DisplayImageAction(IAIntAirAct intAirAct)
        {
            Action<Image, Device> ac = delegate(Image img, Device dev)
            {
                Console.WriteLine(String.Format("Displaying {0} of Device {1}", img, dev));
            };

            Put["action/displayImage"] = _ => Response.Execute(intAirAct, ac);
        }
    }
}
