using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using IntAirAct;
using System.Windows.Forms;

namespace IntAirActImageWindows
{
    public class DisplayImageAction : NancyModule
    {
        public DisplayImageAction(IAIntAirAct intAirAct, PictureBox pictureBox)
        {
            Action<Image, Device> action = delegate(Image img, Device dev)
            {
                Console.WriteLine(String.Format("Displaying {0} of Device {1}", img, dev));

                //string url = intAirAct.GetResourcePath(dev, img);

                string url = String.Format("http://{0}:{1}/image/{2}.jpg", dev.host, dev.port, img.identifier);
                pictureBox.LoadAsync(url);
            };

            Put["action/displayImage"] = _ => Response.Execute(action);
        }
    }
}
