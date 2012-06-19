using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace IntAirActDemoWindow
{
    public class DisplayImageAction : NancyModule
    {
        public DisplayImageAction()
        {
            Put["action/displayImage"] = parameters =>
            {
                return (Response)"Hello";
            };
        }
    }
}
