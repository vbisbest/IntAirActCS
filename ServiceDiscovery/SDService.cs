using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceDiscovery
{
    public class SDService
    {
        public String Name { get; private set; }
        public String Hostname { get; private set; }
        public ushort Port { get; private set; }
        public String Type { get; private set; }
        public Dictionary<String, byte[]> TXTRecord { get; private set; }
    }
}
