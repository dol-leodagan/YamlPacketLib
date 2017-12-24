using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using YamlPacket;

namespace Generator
{
    public class CSharpGenerator : ILanguageGenerator
    {
        static readonly string FileHeader = $"/* This File is Auto-generated */{Environment.NewLine}using System;{Environment.NewLine}{Environment.NewLine}using DOL.PacketLib.{ePacketFrom.Structure};{Environment.NewLine}";
        static readonly string ClassHeader = $"namespace DOL.PacketLib.{{0}}{Environment.NewLine}{{{{{Environment.NewLine}    public sealed class {{1}}{Environment.NewLine}    {{{{";
        static readonly string ClassFooter = $"    }}{Environment.NewLine}}}";
        static readonly Dictionary<string, NumberPrimitive> NumberPrimitives = new Dictionary<string, NumberPrimitive>
        {
            ["number"] = new NumberPrimitive{ IsInteger = true, IsLowEndian = false, IsSigned = false },
            ["low endian number"] = new NumberPrimitive{ IsInteger = true, IsLowEndian = true, IsSigned = false },
            ["signed number"] = new NumberPrimitive{ IsInteger = true, IsLowEndian = false, IsSigned = true },
            ["low endian signed number"] = new NumberPrimitive{ IsInteger = true, IsLowEndian = false, IsSigned = true },

            ["float"] = new NumberPrimitive{ IsInteger = false, IsLowEndian = false, IsSigned = true },
            ["low endian float"] = new NumberPrimitive{ IsInteger = false, IsLowEndian = true, IsSigned = true },
        };

        public CSharpGenerator()
        {
        }

        class NumberPrimitive
        {
            public bool IsInteger;
            public bool IsLowEndian;
            public bool IsSigned;
        }

        string GetPrimitiveFromType(bool isInteger, bool IsSigned, long size)
        {
            switch(size)
            {
                case 1:
                    if (isInteger)
                    {
                        if (IsSigned)
                        {
                            return "sbyte";
                        }
                        else
                        {
                            return "byte";
                        }
                    }
                break;
                case 2:
                    if (isInteger)
                    {
                        if (IsSigned)
                        {
                            return "short";
                        }
                        else
                        {
                            return "ushort";
                        }
                    }
                break;
                case 4:
                    if (isInteger)
                    {
                        if (IsSigned)
                        {
                            return "int";
                        }
                        else
                        {
                            return "uint";
                        }
                    }
                    else
                    {
                        return "float";
                    }
                case 8:
                    if (isInteger)
                    {
                        if (IsSigned)
                        {
                            return "long";
                        }
                        else
                        {
                            return "ulong";
                        }
                    }
                    else
                    {
                        return "double";
                    }
            }

            throw new ApplicationException("Primitive Field has invalid configuration");
        }

        string GetTypeDefinition(IEnumerable<YamlPacketField> fields, IEnumerable<string> structures, out string header)
        {
            header = string.Empty;

            var groupType = fields.GroupBy(field => field.Type).ToArray();

            // check for structure type
            if (groupType.Any(group => structures.Any(strut => strut.Equals(group.Key))))
            {
                var single = groupType.Single();
                if (single.GroupBy(field => field.Count).Single().Key != null)
                {
                    return $"{single.Key}[]";
                }
                return single.Key;
            }

            // check for enumeration type
            if (groupType.Any(group => group.Key.Equals("enumeration", StringComparison.OrdinalIgnoreCase)))
            {
                var enumeration = groupType.Single().Single();
                var values = string.Join("", enumeration.KeyValues.Select(val => string.Format("            {0} = 0x{1:X},{2}",
                    val.Value.Replace(" ", ""),
                    val.Key,
                    Environment.NewLine)));
                header = $"        public enum e{enumeration.Name}{Environment.NewLine}        {{{Environment.NewLine}{values}        }}{Environment.NewLine}";
                return $"e{enumeration.Name}";
            }

            // check for flag type
            if (groupType.Any(group => group.Key.Equals("flags", StringComparison.OrdinalIgnoreCase)))
            {
                var enumeration = groupType.Single().Single();
                var values = string.Join("", enumeration.KeyValues.Select(val => string.Format("            {0} = 0x{1:X},{2}",
                    val.Value.Replace(" ", ""),
                    val.Key,
                    Environment.NewLine)));
                header = $"        [Flags]{Environment.NewLine}        public enum e{enumeration.Name}{Environment.NewLine}        {{{Environment.NewLine}{values}        }}{Environment.NewLine}";
                return $"e{enumeration.Name}";
            }

            // check for string type
            if (groupType.Any(group => group.Key.Equals("string", StringComparison.OrdinalIgnoreCase)))
            {
                var single = groupType.Single();
                return "string";
            }

            // check for char type
            if (groupType.Any(group => group.Key.Equals("char", StringComparison.OrdinalIgnoreCase)))
            {
                var single = groupType.Single();
                return "char";
            }

            // check for bool type
            if (groupType.Any(group => group.Key.Equals("boolean", StringComparison.OrdinalIgnoreCase)))
            {
                var single = groupType.Single();
                return "bool";
            }

            // check for binary type
            if (groupType.Any(group => group.Key.Equals("binary", StringComparison.OrdinalIgnoreCase)))
            {
                var single = groupType.Single();
                return "byte[]";
            }

            // check for primitive
            if (groupType.All(group => NumberPrimitives.Keys.Any(key => key.Equals(group.Key, StringComparison.OrdinalIgnoreCase))))
            {
                var typePrimitives = groupType.Select(group => NumberPrimitives[group.Key]).ToArray();
                if (typePrimitives.All(primitive => primitive.IsInteger && !primitive.IsSigned))
                {
                    return GetPrimitiveFromType(true, false, groupType.SelectMany(type => type).Max(field => field.Size));

                }
                if (typePrimitives.All(primitive => primitive.IsInteger && primitive.IsSigned))
                {
                    return GetPrimitiveFromType(true, true, groupType.SelectMany(type => type).Max(field => field.Size));

                }
                if (groupType.SelectMany(type => type).Max(field => field.Size) <= 4 && typePrimitives.All(primitive => primitive.IsInteger))
                {
                    return GetPrimitiveFromType(true, true, groupType.SelectMany(type => type).Max(field => field.Size) * 2);
                }
                if (groupType.SelectMany(type => type).Max(field => field.Size) <= 4 && typePrimitives.All(primitive => !primitive.IsInteger))
                {
                    return GetPrimitiveFromType(false, true, 4);
                }
                return GetPrimitiveFromType(false, true, 8);
            }

            throw new ApplicationException("Field could not be resolved to a known Type");
        }

        string GetFieldDefinition(IGrouping<string, YamlPacketField> fields, IEnumerable<string> structures)
        {
            try
            {
                var field = fields.Key;
                string header;
                var type = GetTypeDefinition(fields, structures, out header);
                return $"{header}        public {type} {field};";
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error while resolving Field {fields.Key}", ex);
            }
        }

        public void Generate(DirectoryInfo store, IList<YamlPacketRoot> packets)
        {
            var structures = packets.Where(pack => pack.From == ePacketFrom.Structure)
                .GroupBy(pack => pack.Name)
                .Select(structs => structs.Key)
                .ToArray();

            foreach (var packet in packets.GroupBy(pack => new { pack.Name, pack.From }))
            {
                try
                {
                    var storeDir = new DirectoryInfo(Path.Combine(store.FullName, packet.Key.From.ToString()));

                    if (!storeDir.Exists)
                    {
                        storeDir.Create();
                    }

                    var output = new FileInfo(Path.Combine(storeDir.FullName, string.Format("{0}.cs", packet.Key.Name)));

                    using (var writer = output.CreateText())
                    {
                        writer.WriteLine(FileHeader);
                        writer.WriteLine(string.Format(ClassHeader, packet.Key.From, packet.Key.Name));

                        // Class Body
                        foreach (var fields in packet.SelectMany(pack => pack.Fields).GroupBy(field => field.Name))
                        {
                            writer.WriteLine(GetFieldDefinition(fields, structures));
                        }

                        writer.WriteLine(ClassFooter);
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error while parsing {packet.Key.From}.{packet.Key.Name}", ex);
                }
            }
        }
    }
}