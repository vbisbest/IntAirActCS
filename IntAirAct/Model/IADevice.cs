using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroConf;

namespace IntAirAct
{
    public class IADevice : Service
    {
        public HashSet<IARoute> SupportedRoutes { get; private set; }

        public IADevice(string name, string host, ushort port) : base(name, host, port)
        {
            this.SupportedRoutes = new HashSet<IARoute>();
        }

        public override string ToString()
        {
            return String.Format("IADevice[name: {0}, host: {1}, port: {2}, capabilities: {3}]", name, host, port, this.SupportedRoutes);
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

            if (this.SupportedRoutes.Equals(p.SupportedRoutes) && base.Equals(p))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return SupportedRoutes.GetHashCode() ^ base.GetHashCode();
        }
    }
}
