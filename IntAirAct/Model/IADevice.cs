using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class IADevice
    {
        public String Name { get; private set; }
        public String Host { get; private set; }
        public ushort Port { get; private set; }
        public HashSet<IARoute> SupportedRoutes { get; private set; }

        public IADevice(string name, string host, ushort port, HashSet<IARoute> supportedRoutes)
        {
            this.Name = name;
            this.Host = host;
            this.Port = port;
            this.SupportedRoutes = new HashSet<IARoute>();
        }

        public override string ToString()
        {
            return String.Format("IADevice[Name: {0}, Host: {1}, Port: {2}, SupportedRoutes: {3}]", this.Name, this.Host, this.Port, this.SupportedRoutes);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IADevice p = obj as IADevice;
            if ((System.Object)p == null)
            {
                return false;
            }

            if (this.Name.Equals(p.Name))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
