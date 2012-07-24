using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace IntAirAct
{
    public static class ResponseSerializer
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

        public static Response Execute(this IResponseFormatter formatter, Delegate del)
        {
            IAIntAirAct intAirAct = TinyIoC.TinyIoCContainer.Current.Resolve<IAIntAirAct>();

            MethodInfo methodInfo = del.Method;
            ParameterInfo[] pars = methodInfo.GetParameters();
            
            string json = formatter.Context.Request.BodyAsString();
            Console.WriteLine(json);
            JObject action = JObject.Parse(json);
            JArray parameters = (JArray)action["actions"]["parameters"];

            int i = 0;
            List<object> objects = new List<object>();
            foreach (JObject parameter in parameters)
            {
                object obj = intAirAct.DeserializeObject(parameter);
                Type type1 = obj.GetType();
                Type type2 = pars[i].ParameterType;
                if(type1.Equals(type2) || type1.IsSubclassOf(type2))
                {
                    objects.Add(obj);
                }
                i++;
            }

            object returnValue = del.DynamicInvoke(objects.ToArray());

            if (!methodInfo.ReturnType.Equals(typeof(void)))
            {
                IAAction act = new IAAction();
                act.parameters.Add(returnValue);
                return formatter.RespondWith(act, "actions", HttpStatusCode.Created);
            }

            Response res = new Response();
            res.StatusCode = HttpStatusCode.Created;
            return res;
        }
    }
}
