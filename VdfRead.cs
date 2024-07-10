using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace SteamWrap
{
    class VdfRead
    {
        public static List<VdfItem> ParseVdf(string filePath)
        {
            byte[] content = File.ReadAllBytes(filePath);

            // Check for BOM
            int bomOffset = content.Length > 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF ? 3 : 0;

            return ParseBlock(content, ref bomOffset, 0);
        }

        private static List<VdfItem> ParseBlock(byte[] content, ref int startPos, int depth)
        {
            var block = new List<VdfItem>();
            while (startPos < content.Length)
            {
                if (content[startPos] == '"')
                {
                    int keyStart = startPos;
                    int keyEnd = FindUnescapedQuote(content, startPos + 1);
                    string key = VdfStringUtils.UnescapeVdfString(Encoding.UTF8.GetString(content, keyStart + 1, keyEnd - keyStart - 1));
                    startPos = keyEnd + 1;

                    // Skip whitespace
                    while (startPos < content.Length && char.IsWhiteSpace((char)content[startPos]))
                    {
                        startPos++;
                    }

                    if (content[startPos] == '{')
                    {
                        // Nested block
                        startPos++;
                        var nestedBlock = ParseBlock(content, ref startPos, depth + 1);
                        block.Add(new VdfItem
                        {
                            Key = key,
                            ByteStart = keyStart,
                            ByteEnd = startPos,
                            Nested = nestedBlock
                        });
                        startPos++;
                    }
                    else if (content[startPos] == '"')
                    {
                        // Key-value pair
                        int valueStart = startPos;
                        int valueEnd = FindUnescapedQuote(content, startPos + 1);
                        string value = VdfStringUtils.UnescapeVdfString(Encoding.UTF8.GetString(content, valueStart + 1, valueEnd - valueStart - 1));
                        block.Add(new VdfItem
                        {
                            Key = key,
                            Value = value,
                            ByteStart = keyStart,
                            ByteEnd = valueEnd + 1
                        });
                        startPos = valueEnd + 1;
                    }
                }
                else if (content[startPos] == '}')
                {
                    return block;
                }
                else
                {
                    startPos++;
                }
            }
            return block;
        }

        private static int FindUnescapedQuote(byte[] content, int startPos)
        {
            bool escaped = false;
            for (int i = startPos; i < content.Length; i++)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (content[i] == '\\')
                {
                    escaped = true;
                }
                else if (content[i] == '"')
                {
                    return i;
                }
            }
            throw new FormatException("Unterminated string in VDF content");
        }


        public static VdfItem FindKeyContent(List<VdfItem> data, string targetKey)
        {
            foreach (var item in data)
            {
                if (item.Key == targetKey)
                {
                    return item;
                }
                if (item.Nested != null)
                {
                    var result = FindKeyContent(item.Nested, targetKey);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static VdfItem FindGameKey(List<VdfItem> data, string gameId)
        {
            var path = new[] { "UserLocalConfigStore", "Software", "Valve", "Steam", "apps", gameId };
            VdfItem current = null;

            foreach (var key in path)
            {
                if (current == null)
                {
                    current = data.FirstOrDefault(item => item.Key == key);
                }
                else
                {
                    current = current.Nested?.FirstOrDefault(item => item.Key == key);
                }

                if (current == null) return null;
            }

            return current;
        }
    }
}