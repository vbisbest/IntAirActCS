using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Routing;
using System.IO;

namespace IntAirAct
{
    public class NancyRouteModule : NancyModule
    {
        public NancyRouteModule(NancyServerAdapter adapter)
        {
            foreach (KeyValuePair<IARoute, Action<IARequest, IAResponse>> kvp in adapter.Routes)
            {
                IARoute route = kvp.Key;
                Action<IARequest, IAResponse> action = kvp.Value;
                RouteBuilder rb = new RouteBuilder(route.Action, this);
                rb[route.Resource] = x =>
                {
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    foreach (string key in x)
                    {
                        parameters.Add(key, x[key]);
                    }
                    string contentType = "";
                    IADevice origin = null;
                    if (Request.Headers.Keys.Contains("X-IA-Origin"))
                    {
                        IAIntAirAct intAirAct = TinyIoC.TinyIoCContainer.Current.Resolve<IAIntAirAct>();
                        if (intAirAct != null)
                        {
                            origin = intAirAct.DeviceWithName(Request.Headers["X-IA-Origin"].First());
                        }
                    }
                    IARequest iaRequest = new IARequest(route, metadata, parameters, origin, Request.BodyAsByte(), contentType);
                    IAResponse iaResponse = new IAResponse();
                    action(iaRequest, iaResponse);
                    Response response = new Response();
                    response.StatusCode = (HttpStatusCode)iaResponse.StatusCode;
                    response.Contents = stream =>
                    {
                        var writer = new BinaryWriter(stream);
                        writer.Write(iaResponse.Body);
                        writer.Flush();
                    };
                    response.Headers = iaResponse.Metadata;

                    response.ContentType = iaResponse.ContentType;

                    return response;
                };
            }
        }
    }
}
