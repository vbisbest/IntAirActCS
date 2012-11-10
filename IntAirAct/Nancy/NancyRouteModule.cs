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
                    IARequest iaRequest = new IARequest(route, null, null, null);
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
                    return response;
                };
            }
        }
    }
}
