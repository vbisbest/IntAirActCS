using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroConf;

namespace IntAirAct
{
    public class Device : Service
    {
        public HashSet<Capability> capabilities { get; set; }

        public Device(string name, string host, ushort port) : base(name, host, port)
        {
            capabilities = new HashSet<Capability>();
        }

        public override string ToString()
        {
            return String.Format("Device[name: {0}, host: {1}, port: {2}, capabilities: {3}]", name, host, port, capabilities);
        }
    }
}
