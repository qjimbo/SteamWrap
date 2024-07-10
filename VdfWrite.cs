using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SteamWrap
{
    public static class VdfWrite
    {
        public static void VdfWriteItem(VdfItem vdfItem, string targetFilename, string originalFilename, bool matchIndentation = true)
        {
            string SerializeVdfItem(VdfItem item, int indent, bool isRoot)
            {
                var result = new StringBuilder();
                string indentation = new string('\t', indent);

                if (isRoot)
                {
                    result.AppendLine($"\"{VdfStringUtils.EscapeVdfString(item.Key)}\"");
                    result.AppendLine($"{indentation}{{");
                }
                else
                {
                    result.AppendLine($"{indentation}\"{VdfStringUtils.EscapeVdfString(item.Key)}\"");
                    result.AppendLine($"{indentation}{{");
                }

                if (item.Nested != null)
                {
                    foreach (var nestedItem in item.Nested)
                    {
                        if (nestedItem.Nested != null && nestedItem.Nested.Count > 0)
                        {
                            result.Append(SerializeVdfItem(nestedItem, indent + 1, false));
                        }
                        else
                        {
                            result.AppendLine($"{indentation}\t\"{VdfStringUtils.EscapeVdfString(nestedItem.Key)}\"\t\t\"{VdfStringUtils.EscapeVdfString(nestedItem.Value)}\"");
                        }
                    }
                }

                result.Append($"{indentation}}}\n");
                return result.ToString();
            }

            // Read the original file
            byte[] fileContent = File.ReadAllBytes(originalFilename);

            // Find the original indentation
            int originalIndent = FindOriginalIndentation(fileContent, vdfItem.ByteStart);

            string serializedItem = SerializeVdfItem(vdfItem, matchIndentation ? originalIndent : 0, true);

            // Combine the before data, new serialized item, and after data
            using (var ms = new MemoryStream())
            {
                // Write content up to the start of the key (excluding the key itself)
                ms.Write(fileContent, 0, vdfItem.ByteStart);

                // Write the serialized item
                byte[] serializedBytes = Encoding.UTF8.GetBytes(serializedItem);
                ms.Write(serializedBytes, 0, serializedBytes.Length);

                // Find the next line after ByteEnd
                int nextLineStart = vdfItem.ByteEnd;
                while (nextLineStart < fileContent.Length && fileContent[nextLineStart] != '\n')
                {
                    nextLineStart++;
                }
                if (nextLineStart < fileContent.Length)
                {
                    nextLineStart++; // Move past the newline character
                }

                // Write the remaining content
                ms.Write(fileContent, nextLineStart, fileContent.Length - nextLineStart);
                
                // Write the new content to the target file
                File.WriteAllBytes(targetFilename, ms.ToArray());
            }

            Console.WriteLine($"Updated VDF has been written to {targetFilename}");
        }

        private static int FindOriginalIndentation(byte[] content, int byteStart)
        {
            int indentCount = 0;
            for (int i = byteStart - 1; i >= 0; i--)
            {
                if (content[i] == '\n')
                {
                    break;
                }
                if (content[i] == '\t')
                {
                    indentCount++;
                }
            }
            return indentCount;
        }
    }
}
