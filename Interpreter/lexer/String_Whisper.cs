using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            return input.Substring(0, text.Length) == text
                ? new Rune(this, text)
                : null;
        }
    }
}
