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
        public DisplayImageAction(PictureBox pictureBox)
        {
            Action<Image, IADevice> action = delegate(Image img, IADevice dev)
            {
                Console.WriteLine(String.Format("Displaying {0} of Device {1}", img, dev));

                string url = String.Format("http://{0}:{1}/image/{2}.jpg", dev.Host, dev.Port, img.identifier);
                pictureBox.LoadAsync(url);
            };

            Put["action/displayImage"] = _ => Response.Execute(action);
        }
    }
}
