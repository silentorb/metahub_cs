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
        public Rhyme rhyme;
        public Rhyme divider;

        public Repetition_Rhyme(string name)
            : base(name)
        {

        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {
            if (pattern.type != "repetition")
                pattern = pattern.patterns[0];

            var patterns = pattern.patterns[2].patterns;
            rhyme = parser.get_whisper_rhyme(patterns[0].text);
            if (patterns.Length == 3)
            {
                min = int.Parse(patterns[1].text);
                max = int.Parse(patterns[2].text);  
            }
            else
            {
                divider = parser.get_whisper_rhyme(patterns[1].text);
                min = int.Parse(patterns[2].text);
                max = int.Parse(patterns[3].text);   
            }
        }

        public override Legend_Result match(Runestone stone)
        {
            throw new NotImplementedException();
        }
    }
}
