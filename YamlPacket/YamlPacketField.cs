using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlPacket
{
    public class YamlPacketField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public Count Count { get; set; }
        public bool Optional { get; set; }
        public IList<IDictionary<long, string>> Values
        {
            get
            {
                return null;
            }
            set
            {
                KeyValues = value.Select(dict => dict.Single()).ToArray();
            }
        }
        public IList<KeyValuePair<long, string>> KeyValues { get; protected set; }
        public string BitRange { get; set; }
    }

    public class Count
    {
        public string Type { get; set; }
        public long Size { get; set; }
    }
}