using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.parser.rhymes
{
    public class Or_Rhyme : Rhyme
    {
        public List<Rhyme> rhymes;

        public Or_Rhyme(string name)
            : base(name)
        {

        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {
            rhymes = pattern.patterns.Select(p => parser.create_child(p)).ToList();
        }

        public override Legend_Result match(lexer.Runestone stone)
        {
            throw new NotImplementedException();
        }
    }
}
