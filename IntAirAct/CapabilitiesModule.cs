using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Newtonsoft.Json;

namespace IntAirAct
{
    public class CapabilitiesModule : NancyModule
    {
        public CapabilitiesModule(IAIntAirAct intAirAct)
        {
            Get["/capabilities"] = parameters =>
            {
                Dictionary<Object, Object> dic = new Dictionary<Object, Object>();
                dic.Add("capabilities", intAirAct.capabilities);

                Response response = Response.AsJson<Dictionary<Object, Object>>(dic);
                response.WithContentType("application/json;charset=utf-8");
                response.WithHeader("Server", "Hello");
                return response;
            };
        }

    }
}
