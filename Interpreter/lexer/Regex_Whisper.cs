using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace runic.lexer
{
    class Regex_Whisper : Whisper
    {
        public Regex regex;

        public Regex_Whisper(string name, string pattern)
            : base(name)
        {
            regex = new Regex("\\G" + pattern);
        }

        public override Rune match(string input, int position)
        {
            var match = regex.Match(input, position);
            return match.Success 
                ? new Rune(this, match.Value) 
                : null;
        }
    }
}
