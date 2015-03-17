using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    [DebuggerDisplay("Rune {text}")]
    public class Rune
    {
        public Whisper whisper;
        public string text;
        public Range range;

        public Rune(Whisper whisper, string text, Position position)
        {
            this.whisper = whisper;
            this.text = text;
            range = new Range(position, new Position() { position + text.Length)};
        }

        public int length
        {
            get { return text != null ? text.Length : 0; }
        }

    }
}
