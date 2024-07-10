using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWrap
{
    public class VdfItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int ByteStart { get; set; }
        public int ByteEnd { get; set; }
        public List<VdfItem> Nested { get; set; }
    }
}
