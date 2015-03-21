using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

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

        public override Legend_Result match(Runestone stone, Rhyme parent)
        {
            foreach (var rhyme in rhymes)
            {
                var result = rhyme.match(stone, this);
                if (result != null)
                    return result;
            }

            return null;
        }

        override public IEnumerable<Rhyme> aggregate()
        {
            return rhymes;
        }

        public override Rhyme get_single_type()
        {
            var single_type = rhymes[0].get_single_type();
            return rhymes.All(r => r.get_single_type() == single_type)
                ? single_type
                : null;
        }
    }
}
