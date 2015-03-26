using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace runic.lexer
{
    public class Regex_Whisper : Whisper
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

            var value = match.Groups[match.Groups.Count - 1].Value;
            if (value == "")
            {
                if (match.Groups.Count == 2)
                    throw new Exception("Invalid regex");

                value = match.Groups[match.Groups.Count - 2].Value;
            }
            return new Rune(this, value, position.clone(), position.forward(match.Length).clone());
        }
    }
}
