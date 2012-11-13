using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class IARequest
    {
        public IARoute Route { get; private set; }
        public Dictionary<String, String> Metadata { get; private set; }
        public Dictionary<String, String> Parameters { get; private set; }
        public byte[] Body { get; private set; }

        public IARequest(IARoute route, Dictionary<String, String> metadata, Dictionary<String, String> parameters, byte[] body)
        {
            this.Route = route;
            this.Metadata = metadata;
            this.Parameters = parameters;
            this.Body = body;
        }

        public override string ToString()
        {
            return String.Format("IARequest[Route: {0}, Metadata: {1}, Parameters: {2}, Body: {3}]", Route, Metadata, Parameters, Body);
        }

        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || (obj as IARequest) == null)
            {
                return false;
            }

            IARequest request = (IARequest)obj;
            return (this.Route == request.Route || (this.Route != null && this.Route.Equals(request.Route)))
                && (this.Metadata == request.Metadata || (this.Metadata != null && this.Metadata.Equals(request.Metadata)))
                && (this.Parameters == request.Parameters || (this.Parameters != null && this.Parameters.Equals(request.Parameters)))
                && (this.Body == request.Body);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IARequest p = obj as IARequest;
            if ((System.Object)p == null)
            {
                return false;
            }

            if (this.Route.Equals(p.Route) && this.Metadata.Equals(p.Metadata) && this.Parameters.Equals(p.Parameters) && this.Body.Equals(p.Body))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 89;
            hash = hash * 31 + (this.Route == null ? 0 : this.Route.GetHashCode());
            hash = hash * 31 + (this.Metadata == null ? 0 : this.Metadata.GetHashCode());
            hash = hash * 31 + (this.Parameters == null ? 0 : this.Parameters.GetHashCode());
            hash = hash * 31 + (this.Body == null ? 0 : this.Body.GetHashCode());
            return hash;
        }
    }
}
