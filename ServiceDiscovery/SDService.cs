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
        public Dictionary<String, String> TXTRecord { get; private set; }

        public SDService(String name, String hostname, ushort port, String type, Dictionary<String, String> TXTRecord)
        {
            this.Name = name;
            this.Hostname = hostname;
            this.Port = port;
            this.Type = type;
            this.TXTRecord = TXTRecord;
        }

    }
}
