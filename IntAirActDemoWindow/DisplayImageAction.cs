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

            Func<Image, Image, Image> func = delegate(Image img1, Image img2)
            {
                Image res = new Image();
                res.identifier = img1.identifier + img2.identifier;
                return res;
            };

            Put["action/addImages"] = _ => Response.Execute(intAirAct, func);
        }
    }
}
