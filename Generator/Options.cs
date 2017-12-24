using System;

using CommandLine;
using CommandLine.Text;

namespace Generator
{
    public class Options
    {
        [Option('l', "language", Default = "csharp", HelpText = "Output Language to Generate")]
        public string Language { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output directory for Generated definitions")]
        public string Output { get; set; }
    }
}