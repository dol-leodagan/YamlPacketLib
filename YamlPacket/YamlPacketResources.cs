using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace YamlPacket
{
    public class YamlPacketResources
    {
        public static IList<YamlPacketRoot> ExtractYamlPacketResources()
        {
            return typeof(YamlPacketResources).Assembly.GetManifestResourceNames()
            .Where(res => res.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            .Select(res => YamlPacketRoot.LoadFrom(new StreamReader(typeof(YamlPacketResources).Assembly.GetManifestResourceStream(res))))
            .ToArray();
        }
    }
}