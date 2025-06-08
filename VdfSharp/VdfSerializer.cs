using VdfSharp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VdfSharp
{
    public static class VdfSerializer
    {
        private enum TokenType : byte
        {
            String = 0,
            OpeningBracket = 1,
            ClosingBracket = 2,
        }

        public static VdfProperty Deserialize(string filePath)
        {
            string text = File.ReadAllText(filePath, Encoding.UTF8);

            int i = 0;
            return ReadRootProperty();

            VdfProperty ReadRootProperty()
            {
                ReadNextValidToken(out string? key);
                bool isKeyValue = ReadNextValidToken(out string? nextToken) is TokenType.String;
                if (isKeyValue)
                {
                    // root should not be a kv
                    throw new Exception();
                }
                else
                {
                    return new VdfProperty
                    {
                        Key = key!,
                        Values = ReadPropertyValues(),
                    };
                }
            }

            List<VdfBase> ReadPropertyValues()
            {
                List<VdfBase> values = new List<VdfBase>();
                while (true)
                {
                    TokenType token1 = ReadNextValidToken(out string? key);
                    if (token1 is TokenType.ClosingBracket)
                        break;

                    bool isKeyValue = ReadNextValidToken(out string? value) is TokenType.String;
                    if (isKeyValue)
                    {
                        values.Add(new VdfKeyValue
                        {
                            Key = key!,
                            Value = value!,
                        });
                    }
                    else // value is one or more properties
                    {
                        values.Add(new VdfProperty
                        {
                            Key = key!,
                            Values = ReadPropertyValues(),
                        });
                    }
                }
                return values;
            }

            TokenType ReadNextValidToken(out string? str)
            {
                str = null;
                while (true)
                {
                    switch (text[i++])
                    {
                        case '\t'or '\n':
                            continue; // ignore tabs (whitespace) or newlines
                        case '\"':
                            str = ReadString();
                            return TokenType.String;
                        case '{':
                            return TokenType.OpeningBracket;
                        case '}':
                            return TokenType.ClosingBracket;
                        default:
                            // print info about where the unexpected token occurred
                            int currentCharIndex = i - 1;
                            string[] allLines = text.Split('\n');
                            int currentLineIndex = -1;
                            for (int lineIndex = 0, charCount = 0; lineIndex < allLines.Length; lineIndex++)
                            {
                                charCount += allLines[lineIndex].Length;
                                if (lineIndex > 0)
                                    charCount++;

                                if (charCount > currentCharIndex)
                                {
                                    currentLineIndex = lineIndex;
                                    break;
                                }
                            }

                            string? prevLine = (currentLineIndex - 1) >= 0 ? allLines[currentLineIndex - 1] : null;
                            string currentLine = allLines[currentLineIndex];
                            string? nextLine = (currentLineIndex + 1) < allLines.Length ? allLines[currentLineIndex + 1] : null;

                            throw new Exception($"Unexpected Token." +
                                $"\nToken={text[currentCharIndex]}," +
                                $"\nTokenIndex={currentCharIndex}" +
                                $"\nLineNum={currentLineIndex + 1}," +
                                $"\nPrevLine={prevLine}," +
                                $"\nCurrLine={currentLine}," +
                                $"\nNextLine={nextLine},");
                    }
                }
            }

            string ReadString()
            {
                // read until the next non-escaped double quote
                int startIndex = i;
                int length = 0;
                bool escape = false;
                while (true)
                {
                    char c = text[i++];
                    if (escape)
                    {
                        escape = false;
                    }
                    else if (c == '\\')
                    {
                        escape = true;
                    }
                    else if (c == '\"')
                    {
                        break;
                    }
                    length++;
                }
                return text.Substring(startIndex, length);
            }
        }

        public static void Serialize(VdfProperty rootProperty, string filePath)
        {
            using (StreamWriter writer = File.CreateText(filePath))
            {
                int indent = 0;
                WriteProperty(rootProperty);

                writer.Flush();
                writer.Close();

                void WriteProperty(VdfProperty prop)
                {
                    // write indent + key
                    writer.Write(new string('\t', indent));
                    writer.Write('\"');
                    writer.Write(prop.Key);
                    writer.Write('\"');
                    writer.Write('\n');

                    // write indent + opening bracket
                    writer.Write(new string('\t', indent));
                    writer.Write('{');
                    writer.Write('\n');

                    // write values
                    indent++;
                    foreach (var value in prop.Values)
                    {
                        if (value is VdfKeyValue kv)
                        {
                            WriteKeyValue(kv);
                        }
                        else if (value is VdfProperty childProp)
                        {
                            WriteProperty(childProp);
                        }
                    }
                    indent--;

                    // write indent + closing bracket
                    writer.Write(new string('\t', indent));
                    writer.Write('}');
                    writer.Write('\n');
                }

                void WriteKeyValue(VdfKeyValue kv)
                {
                    // write indent + key
                    writer.Write(new string('\t', indent));
                    writer.Write('\"');
                    writer.Write(kv.Key);
                    writer.Write('\"');

                    // write value
                    writer.Write(new string('\t', 2));
                    writer.Write('\"');
                    writer.Write(kv.Value);
                    writer.Write('\"');
                    writer.Write('\n');
                }
            }
        }
    }
}
