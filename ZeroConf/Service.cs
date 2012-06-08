using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroConf
{
    /// <summary>
    /// A Device represents a Bonjour service found on the network.
    /// </summary>
    public class Service
    {
        public string name;
        public string host;
        public ushort port;

        public Service(String name, String host, ushort port)
        {
            this.name = name;
            this.host = host;
            this.port = port;  // Initialized to zero because we need to resolve to get the port later
        }

        public override string ToString()
        {
            return String.Format("ZeroConfService[name: {0}, host: {1}, port: {2}]", name, host, port);
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType().IsSubclassOf(this.GetType()))
            {
                Service other = (Service) obj;
                return name.Equals(other.name);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
