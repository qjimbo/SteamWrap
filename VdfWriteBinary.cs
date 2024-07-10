using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

public static class VdfWriteBinary
{
    public static void VdfWriteBinaryShortcut(string filePath, string appName, string appIcon, string exePath, string args)
    {
        List<byte> data = new List<byte>();

        if (File.Exists(filePath))
        {
            Console.WriteLine("Reading " + filePath);
            data.AddRange(File.ReadAllBytes(filePath));
            // Remove the last two 0x08 bytes if they exist
            if (data.Count >= 2 && data[data.Count - 1] == 0x08 && data[data.Count - 2] == 0x08)
            {
                data.RemoveRange(data.Count - 2, 2);
            }
        }
        else
        {
            Console.WriteLine("Creating new " + filePath);
            data.AddRange(new byte[] { 0x00, 0x73, 0x68, 0x6F, 0x72, 0x74, 0x63, 0x75, 0x74, 0x73, 0x00 }); // <bh:00>shortcuts<bh:00><bh:00>
        }

        using (MemoryStream ms = new MemoryStream())
        {
            int nextIndex = GetNextIndex(data.ToArray());

            ms.WriteByte(0x00); // Separator
            
            WriteString(ms, nextIndex.ToString());
            ms.WriteByte(0x00);

            WriteKeyValueAsInt(ms, "appid", GenerateAppId(appName));
            WriteKeyValue(ms, "AppName", appName);
            WriteKeyValue(ms, "Exe", $"\"{exePath}\"");
            WriteKeyValue(ms, "StartDir", Path.GetDirectoryName(exePath) + "\\");
            WriteKeyValue(ms, "icon", appIcon);
            WriteKeyValue(ms, "ShortcutPath", "");
            WriteKeyValue(ms, "LaunchOptions", args);
            WriteBooleanKeyValue(ms, "IsHidden", false);
            WriteBooleanKeyValue(ms, "AllowDesktopConfig", true);
            WriteBooleanKeyValue(ms, "AllowOverlay", true);
            WriteBooleanKeyValue(ms, "OpenVR", false);
            WriteBooleanKeyValue(ms, "Devkit", false);
            WriteKeyValue(ms, "DevkitGameID", "");
            WriteBooleanKeyValue(ms, "DevkitOverrideAppID", false);
            WriteKeyValueAsInt(ms, "LastPlayTime", 0);

            // Special handling for FlatpakAppID
            ms.WriteByte(0x01);
            WriteString(ms, "FlatpakAppID");
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            // Special handling for tags
            WriteString(ms, "tags");
            ms.WriteByte(0x00);
            ms.WriteByte(0x08);
            ms.WriteByte(0x08);

            data.AddRange(ms.ToArray());
        }

        // Add the final bytes
        data.Add(0x08);
        data.Add(0x08);

        File.WriteAllBytes(filePath, data.ToArray());
    }

    private static void WriteString(MemoryStream ms, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        ms.Write(bytes, 0, bytes.Length);
    }

    private static void WriteKeyValue(MemoryStream ms, string key, string value)
    {
        ms.WriteByte(0x01);
        WriteString(ms, key);
        ms.WriteByte(0x00);
        WriteString(ms, value);
        ms.WriteByte(0x00);
    }

    private static void WriteKeyValue(MemoryStream ms, string key, byte[] value)
    {
        ms.WriteByte(0x01);
        WriteString(ms, key);
        ms.WriteByte(0x00);
        ms.Write(value, 0, value.Length);
    }

    private static void WriteKeyValueAsInt(MemoryStream ms, string key, int value)
    {
        ms.WriteByte(0x02);
        WriteString(ms, key);
        ms.WriteByte(0x00);
        ms.Write(BitConverter.GetBytes(value), 0, 4);
    }

    private static void WriteBooleanKeyValue(MemoryStream ms, string key, bool value)
    {
        ms.WriteByte(0x02);
        WriteString(ms, key);
        ms.WriteByte(0x00);
        ms.WriteByte((byte)(value ? 0x01 : 0x00));
        ms.WriteByte(0x00);
        ms.WriteByte(0x00);
        ms.WriteByte(0x00);
    }

    private static int GetNextIndex(byte[] data)
    {
        int maxIndex = -1;
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == 0x00 && char.IsDigit((char)data[i + 1]))
            {
                int index = 0;
                i++;
                while (i < data.Length && char.IsDigit((char)data[i]))
                {
                    index = index * 10 + (data[i] - '0');
                    i++;
                }
                maxIndex = Math.Max(maxIndex, index);
            }
        }
        return maxIndex + 1;
    }

    private static int GenerateAppId(string appName)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(appName);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToInt32(hashBytes, 0);
        }
    }
}