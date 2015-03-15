using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser.rhymes
{
    class Single_Rhyme : Rhyme
    {
        public Whisper whisper;

        public Single_Rhyme(string name)
            : base(name)
        {

        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {
            var id = pattern.patterns[0].text;
            whisper = parser.lexer.whispers[id];
        }
    }
}
