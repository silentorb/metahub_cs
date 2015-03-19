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
            : base(Whisper_Type.regex, name)
        {
            regex = new Regex("\\G" + pattern);
        }

        public override Rune match(string input, Position position)
        {
            var match = regex.Match(input, position.index);
            if (!match.Success)
                return null;

            return new Rune(this, match.Value, position.clone(), position.forward(match.Length).clone());
        }
    }
}
