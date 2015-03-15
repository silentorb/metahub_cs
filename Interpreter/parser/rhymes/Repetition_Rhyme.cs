using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser.rhymes
{
    public class Repetition_Rhyme : Rhyme
    {
        public int min; // min < 1 means this pattern is optional
        public int max; // max < 1 is infinite
        public Whisper whisper;
        public Whisper divider;

        public Repetition_Rhyme(string name)
            : base(name)
        {

        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {

        }
    }
}
