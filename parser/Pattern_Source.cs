using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    public class Pattern_Source
    {
        public string type { get; set; }
        public string name { get; set; }
        public string action;
        public string text;
        public bool? backtrack;

        public int? min;
        public int? max;

        public Pattern_Source pattern;
        public Pattern_Source divider;
        public string[] dividers;
        public Pattern_Source[] patterns;
        public Pattern_Source value;
        public Dictionary<string, Pattern_Source> dictionary;
    }
}
