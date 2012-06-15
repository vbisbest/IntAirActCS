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

                string json = JsonConvert.SerializeObject(dic);

                Response resp = (Response)json;
                resp.WithContentType("application/json;charset=utf-8");
                resp.WithHeader("Server", "Hello");
                resp.Headers.Remove("Date");
                return resp;
            };
        }

    }
}
