using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntAirAct
{
    public class IADeSerialization
    {
        public byte[] Body { get; set; }
        public string ContentType { get; set; }

        public IADeSerialization()
        {
            this.Body = new byte[0];
            this.ContentType = "plain/text";
        }

        public IADeSerialization(byte[] body)
        {
            this.Body = body;
            this.ContentType = "plain/text";
        }

        public dynamic BodyAs<T>()
        {
            Type type = typeof(T);
            if (type.IsAssignableFrom(typeof(string)))
            {
                return BodyAsString();
            }
            else
            {
                object data = JsonConvert.DeserializeObject(BodyAsString());
                return Deserialize<T>(data);
            }
        }

        private dynamic Deserialize<T>(object data)
        {
            if (data.GetType().IsAssignableFrom(typeof(JArray)))
            {
                List<T> result = new List<T>();
                foreach (object item in (JArray)data)
                {
                    result.Add(Deserialize<T>(item));
                }
                return result;
            } else if (data.GetType().IsAssignableFrom(typeof(JObject))) {
                JObject jObject = (JObject)data;
                return jObject.ToObject<T>();
            }
            return null;
        }

        public string BodyAsString()
        {
            return System.Text.Encoding.UTF8.GetString(this.Body);
        }

        public void SetBodyWith(Object data)
        {
            if (data == null)
            {
                return;
            }
            else if (data.GetType() == typeof(string))
            {
                SetBodyWithString((string)data);
            }
            else
            {
                String json = JsonConvert.SerializeObject(data);

                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                this.Body = encoding.GetBytes(json);

                this.ContentType = "application/json";
            }
        }

        public void SetBodyWithString(string body)
        {
            this.Body = System.Text.Encoding.UTF8.GetBytes(body);
        }
    }
}
