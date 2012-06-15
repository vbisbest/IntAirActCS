using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nancy;

namespace IntAirAct
{
    public class HelloModule : NancyModule
    {
        public HelloModule(Hello hello)
        {
            Get["/hello"] = parameters =>
            {
                Response resp = (Response)hello.hello();
                resp.WithContentType("application/json;charset=utf-8");
                resp.WithHeader("Server", "Hello");
                resp.Headers.Remove("Date");
                return resp;
            }; 
        }

    }
}
