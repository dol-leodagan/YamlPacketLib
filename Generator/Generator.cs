using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using YamlDotNet.Serialization;
using YamlPacket;

namespace Generator
{
    public class Generator
    {
        enum eLanguage
        {
            csharp,
        }

        DirectoryInfo Output { get; set; }
        eLanguage Language { get; set; }

        public Generator(Options Options)
        {
            Output = new DirectoryInfo(Options.Output);

            Language = Enum.Parse<eLanguage>(Options.Language);
        }

        public void Generate()
        {
            if (!Output.Exists)
            {
                Output.Create();
            }

            var packets = YamlPacketResources.ExtractYamlPacketResources();

            var languageGenerator = GetGenerator();

            languageGenerator.Generate(Output, packets);
        }

        ILanguageGenerator GetGenerator()
        {
            switch (Language)
            {
                case eLanguage.csharp:
                    return new CSharpGenerator();
                default:
                    return null;
            }
        }
    }
}