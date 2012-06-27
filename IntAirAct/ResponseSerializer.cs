using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Newtonsoft.Json;

namespace IntAirAct
{
    static class ResponseSerializer
    {
        public static Response RespondWith(this IResponseFormatter formatter, Object model, string keypath, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Dictionary<Object, Object> dic = new Dictionary<Object, Object>();
            dic.Add(keypath, model);

            Response response = JsonConvert.SerializeObject(dic);
            response.WithContentType("application/json;charset=utf-8");
            response.WithHeader("Server", "Hello");
            return response;
        }
    }
}
