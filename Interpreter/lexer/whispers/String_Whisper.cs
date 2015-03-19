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
            : base(Whisper_Type.text, name)
        {
            this.text = text;
        }

        public override Rune match(string input, Position position)
        {
            var slice = Lexer.get_safe_substring(input, position.index, text.Length);
            return slice == text
                ? new Rune(this, text, position.clone(), position.forward(text.Length).clone())
                : null;
        }
    }
}
