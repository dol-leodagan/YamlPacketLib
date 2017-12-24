using System;

using CommandLine;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options => {
                try
                {
                    new Generator(options).Generate();
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine("Errors: {0}", ex);
                    Environment.Exit(1);
                }
            });
        }
    }
}
