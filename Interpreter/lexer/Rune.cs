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

        public Rune(Whisper whisper, string text, Position start, Position end)
        {
            this.whisper = whisper;
            this.text = text;
            this.range = new Range(start, end);
        }

        public int length
        {
            get { return text != null ? text.Length : 0; }
        }

    }
}
