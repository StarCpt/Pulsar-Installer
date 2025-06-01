using VdfSharp.Data;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace VdfSharp
{
    public static class BinaryVdf
    {
        private enum StructType : byte
        {
            Map    = 0x00,
            String = 0x01,
            Number = 0x02,
            MapEnd = 0x08,
        }

        public static BinaryVdfMap Deserialize(string filePath)
        {
            byte[] vdfBytes = File.ReadAllBytes(filePath);

            BinaryVdfMap root = null;
            Stack<BinaryVdfMap> mapstack = new Stack<BinaryVdfMap>(); // popped are added to the next map's values

            int i = 0;
            while (i < vdfBytes.Length && root is null)
            {
                switch ((StructType)vdfBytes[i++])
                {
                    case StructType.Map:
                        mapstack.Push(new BinaryVdfMap
                        {
                            Key = ReadString(),
                            Values = new List<BinaryVdfBase>(),
                        });
                        break;
                    case StructType.String:
                        mapstack.Peek().Values.Add(new BinaryVdfString
                        {
                            Key = ReadString(),
                            Value = ReadString(),
                        });
                        break;
                    case StructType.Number:
                        mapstack.Peek().Values.Add(new BinaryVdfNumber
                        {
                            Key = ReadString(),
                            Value = ReadNumber(),
                        });
                        break;
                    case StructType.MapEnd:
                        if (mapstack.Count > 1)
                        {
                            BinaryVdfMap popped = mapstack.Pop();
                            mapstack.Peek().Values.Add(popped);
                        }
                        else
                        {
                            root = mapstack.Pop();
                        }
                        break;
                    default:
                        throw new Exception("VDF deserialization failed, the file may be corrupt.");
                }
            }

            if (root is null)
                throw new Exception("VDF deserialization failed, the file may be corrupt.");

            return root;

            string ReadString()
            {
                List<char> characters = new List<char>(16);
                while (i < vdfBytes.Length)
                {
                    byte b = vdfBytes[i++];

                    if (b is 0x00) // strings are null-terminated
                        break;

                    characters.Add((char)b);
                }

                return new string(characters.ToArray());
            }

            uint ReadNumber()
            {
                uint num = BinaryPrimitives.ReadUInt32LittleEndian(vdfBytes.AsSpan(i, 4));
                i += 4;
                return num;
            }
        }

        public static void Serialize(BinaryVdfMap value, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
