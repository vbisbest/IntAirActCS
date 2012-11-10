using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroConf;

namespace IntAirAct
{
    public class IADevice : Service
    {
        public HashSet<IARoute> SupportedRoutes { get; set; }

        public IADevice(string name, string host, ushort port) : base(name, host, port)
        {
            this.SupportedRoutes = new HashSet<IARoute>();
        }

        public override string ToString()
        {
            return String.Format("Device[name: {0}, host: {1}, port: {2}, capabilities: {3}]", name, host, port, this.SupportedRoutes);
        }
    }
}
