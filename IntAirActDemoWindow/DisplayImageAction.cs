using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Nancy.IO;
using System.IO;

namespace IntAirActDemoWindow
{
    public class DisplayImageAction : NancyModule
    {
        public DisplayImageAction()
        {
            Put["action/displayImage"] = parameters =>
            {
                RequestStream bodyStream = this.Context.Request.Body;
                bodyStream.Position = 0;
                string bodyText;
                using (var bodyReader = new StreamReader(bodyStream))
                {
                    bodyText = bodyReader.ReadToEnd();
                }
                Console.WriteLine(bodyText);
                return (Response)"Hello";
            };
        }
    }
}
