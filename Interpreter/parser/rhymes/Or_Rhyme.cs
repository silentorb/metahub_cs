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
            : base(Rhyme_Type.or, name)
        {

        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {
            rhymes = pattern.patterns.Select(p => parser.create_child(p)).ToList();
        }

        public override Legend_Result match(lexer.Runestone stone)
        {
            foreach (var rhyme in rhymes)
            {
                var result = rhyme.match(stone);
                if (result != null)
                    return result;
            }

            return null;
        }

        override public IEnumerable<Rhyme> aggregate()
        {
            return rhymes;
        }
    }
}
