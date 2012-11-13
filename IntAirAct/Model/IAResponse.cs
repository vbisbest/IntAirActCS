using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace IntAirAct
{
    public class IAResponse
    {
        public int StatusCode { get; set; }
        public byte[] Body { get; set; }
        public Dictionary<String, String> Metadata { get; set; }

        public IAResponse()
        {
            this.StatusCode = 200;
            this.Body = new byte[0];
            this.Metadata = new Dictionary<string,string>();
        }

        public void RespondWith(Object model, string keypath, int statusCode = 200)
        {
            this.StatusCode = statusCode;

            Dictionary<Object, Object> dic = new Dictionary<Object, Object>();
            dic.Add(keypath, model);

            String json = JsonConvert.SerializeObject(dic);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            this.Body = encoding.GetBytes(json);

            this.Metadata.Add("Content-Type", "application/json;charset=utf-8");
            this.Metadata.Add("Server", "Hello");
        }

        public override string ToString()
        {
            return String.Format("IAResponse[StatusCode: {0}, Body: {1}, Metadata: {2}]", StatusCode, Body, Metadata);
        }

        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || (obj as IAResponse) == null)
            {
                return false;
            }

            IAResponse response = (IAResponse)obj;
            return (this.StatusCode == response.StatusCode)
                && (this.Body == response.Body)
                && (this.Metadata == response.Metadata || (this.Metadata != null && this.Metadata.Equals(response.Metadata)));
        }

        public override int GetHashCode()
        {
            int hash = 53;
            hash = hash * 31 + this.StatusCode.GetHashCode();
            hash = hash * 31 + (this.Body == null ? 0 : this.Body.GetHashCode());
            hash = hash * 31 + (this.Metadata == null ? 0 : this.Metadata.GetHashCode());
            return hash;
        }
    }
}
