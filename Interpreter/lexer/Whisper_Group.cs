using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    public class Whisper_Group : Whisper
    {
        public Whisper[] whispers;

        public Whisper_Group(string name, IEnumerable<Whisper> whispers)
            : base(name)
        {
            this.whispers = whispers.ToArray();
        }

        public override Rune match(string input, int position)
        {
            foreach (var whisper in whispers)
            {
                var rune = whisper.match(input, position);
                if (rune != null)
                    return rune;
            }

            return null;
        }
    }
}
