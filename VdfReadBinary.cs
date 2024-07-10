using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace SteamWrap
{
    public class VdfReadBinary
    {
        public static List<VdfItem> ParseVdf(string filePath)
        {
            byte[] content = File.ReadAllBytes(filePath);
            return ParseBlock(content, 0);
        }

        private static List<VdfItem> ParseBlock(byte[] content, int startPos)
        {
            var block = new List<VdfItem>();
            while (startPos < content.Length - 1)
            {
                byte type = content[startPos];
                if (type == 0x00)
                {
                    // End of block
                    return block;
                }

                string key = ReadNullTerminatedString(content, startPos + 1);
                startPos += key.Length + 2; // +1 for type, +1 for null terminator

                if (type == 0x01) // String type
                {
                    string value = ReadNullTerminatedString(content, startPos);
                    startPos += value.Length + 1; // +1 for null terminator
                    block.Add(new VdfItem
                    {
                        Key = key,
                        Value = value,
                        ByteStart = startPos - key.Length - value.Length - 3,
                        ByteEnd = startPos
                    });
                }
                else if (type == 0x02) // Int32 type
                {
                    int value = BitConverter.ToInt32(content, startPos);
                    startPos += 4;
                    block.Add(new VdfItem
                    {
                        Key = key,
                        Value = value.ToString(),
                        ByteStart = startPos - key.Length - 7,
                        ByteEnd = startPos
                    });
                }
                else if (type == 0x08) // Start of a new block
                {
                    var nestedBlock = ParseBlock(content, startPos);
                    startPos = nestedBlock.Last().ByteEnd + 1; // Skip the ending 0x08 byte
                    block.Add(new VdfItem
                    {
                        Key = key,
                        ByteStart = startPos - key.Length - 2,
                        ByteEnd = startPos,
                        Nested = nestedBlock
                    });
                }
            }
            return block;
        }

        private static string ReadNullTerminatedString(byte[] content, int startPos)
        {
            int endPos = startPos;
            while (endPos < content.Length && content[endPos] != 0)
            {
                endPos++;
            }
            return Encoding.UTF8.GetString(content, startPos, endPos - startPos);
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
            var path = new[] { "shortcuts" };
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

            // In binary VDF, game entries are indexed numerically
            return current.Nested?.FirstOrDefault(item =>
                item.Nested != null &&
                item.Nested.Any(subItem => subItem.Key == "appid" && subItem.Value == gameId));
        }
    }
}