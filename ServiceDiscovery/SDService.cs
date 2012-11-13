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

        public override string ToString()
        {
            return String.Format("SDService[Name: {0}, Hostname: {1}, Port: {2}, Type: {3}, TXTRecord: {4}]", Name, Hostname, Port, Type, TXTRecord);
        }

        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || (obj as SDService) == null)
            {
                return false;
            }

            SDService response = (SDService)obj;
            return (this.Name == response.Name || (this.Name != null && this.Name.Equals(response.Name)));
        }

        public override int GetHashCode()
        {
            int hash = 77;
            hash = hash * 31 + (this.Name == null ? 0 : this.Name.GetHashCode());
            return hash;
        }

    }
}
