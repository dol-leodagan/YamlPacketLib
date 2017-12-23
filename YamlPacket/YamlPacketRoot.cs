using System;
using System.Collections.Generic;
using System.Linq;

using YamlDotNet.Serialization;

namespace YamlPacket
{
    public enum ePacketFrom
    {
        Structure,
        Server,
        Client,
    }

    public class YamlPacketRoot
    {
        public string Name { get; protected set; }
        public byte Code { get; set; }
        public ePacketFrom From { get; set; } = ePacketFrom.Structure;
        public long Version { get; set; }
        public string Revision { get; set; }
        public IList<long> FixedSizes { get; set; }
        public long? FixedSize { get; set; }
        public long? MinimumSize { get; set; }
        public IList<IDictionary<string, YamlPacketField>> Body {
            get
            {
                return null;
            }
            set
            {
                Fields = value.Select(dict => {
                        var fieldNode = dict.Single();
                        fieldNode.Value.Name = fieldNode.Key;
                        return fieldNode.Value;
                    }).ToArray();
            }
        }
        public IList<YamlPacketField> Fields { get; protected set; }

        public static YamlPacketRoot LoadFrom(System.IO.StreamReader reader)
        {
            var deserializer = new Deserializer();
            var rootNode = deserializer.Deserialize<Dictionary<string, YamlPacketRoot>>(reader).Single();
            rootNode.Value.Name = rootNode.Key;
            return rootNode.Value;
        }
    }
}