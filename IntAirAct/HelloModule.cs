using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nancy;

namespace IntAirAct
{
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/hello"] = parameters =>
            {
                Dictionary<Object, Object> message = new Dictionary<Object, Object>();
                message.Add("message", "hello world");

                Dictionary<Object, Object> dic = new Dictionary<Object, Object>();
                dic.Add("responses", message);

                Response response = Response.AsJson<Dictionary<Object, Object>>(dic);
                response.WithContentType("application/json;charset=utf-8");
                response.WithHeader("Server", "Hello");

                return response;
            }; 
        }

    }
}
