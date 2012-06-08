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
                Capability capability = new Capability();
                capability.capability = "hello";

                Capability capability2 = new Capability();
                capability2.capability = "hello";

                HashSet<Object> set = new HashSet<Object>();
                set.Add(capability);
                set.Add(capability2);

                Dictionary<Object, Object> dic = new Dictionary<Object, Object>();
                dic.Add("capabilities", set);

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
