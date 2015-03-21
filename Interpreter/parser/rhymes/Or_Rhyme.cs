using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;
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

        public override void initialize(Pattern_Source pattern, Parser parser)
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

        protected override List<Rhyme> get_single_type()
        {
            var result = rhymes[0].vertical_return_types;
            foreach (var rhyme in rhymes.Skip(1))
            {
                result = result.Except(rhyme.vertical_return_types).ToList();
            }

            if (!result.Contains(this))
                result.Add(this);

            return result;
        }
    }
}
