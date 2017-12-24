using System;
using System.IO;
using System.Collections.Generic;

using YamlPacket;

namespace Generator
{
    public interface ILanguageGenerator
    {
        void Generate(DirectoryInfo store, IList<YamlPacketRoot> packets);
    }
}