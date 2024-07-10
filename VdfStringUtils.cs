using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWrap
{
    public static class VdfStringUtils
    {
        public static string EscapeVdfString(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static string UnescapeVdfString(string input)
        {
            return input.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
