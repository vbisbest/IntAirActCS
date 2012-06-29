using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.IO;
using System.IO;

namespace IntAirAct
{
    public static class RequestAsString
    {
        public static string BodyAsString(this Request request)
        {
            RequestStream bodyStream = request.Body;
            bodyStream.Position = 0;
            string bodyText;
            using (var bodyReader = new StreamReader(bodyStream))
            {
                bodyText = bodyReader.ReadToEnd();
            }
            return bodyText;
        }
    }
}
