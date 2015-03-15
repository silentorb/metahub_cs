using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using interpreter.runic;

namespace runic.lexer
{
    public class String_Whisper : Whisper
    {
        public string text;

        public String_Whisper(string name, string text)
            : base(name)
        {
            this.text = text;
        }

        public override Rune match(string input, int position)
        {
            var slice = Lexer.get_safe_substring(input, position, text.Length);
            return slice == text
                ? new Rune(this, text)
                : null;
        }
    }
}
