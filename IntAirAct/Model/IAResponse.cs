using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            return String.Format("IAResponse[StatusCode: {0}, Body: {1}, Metadata: {2}]", StatusCode, Body, Metadata);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IAResponse p = obj as IAResponse;
            if ((System.Object)p == null)
            {
                return false;
            }

            if (this.StatusCode.Equals(p.StatusCode) && this.Body.Equals(p.Body) && this.Metadata.Equals(p.Metadata))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return StatusCode.GetHashCode() ^ Body.GetHashCode() ^ Metadata.GetHashCode();
        }
    }
}
