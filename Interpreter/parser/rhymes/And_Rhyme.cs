using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;
using runic.lexer;

namespace runic.parser.rhymes
{
    public class And_Rhyme : Rhyme
    {
        public List<Rhyme> rhymes;
        private Rhyme single_non_ghost;

        public And_Rhyme(string name)
            : base(Rhyme_Type.and, name)
        {

        }

        public override void initialize(Pattern_Source pattern, Parser parser)
        {
            rhymes = pattern.patterns.Select(parser.create_child).ToList();

            if (rhymes.Count(r => !r.is_ghost) == 1)
                single_non_ghost = rhymes.First(r => !r.is_ghost);
        }

        public override Legend_Result match(Runestone stone, Rhyme parent)
        {
            var results = new List<Legend>();
            foreach (var rhyme in rhymes)
            {
                var result = rhyme.match(stone, this);
                if (result == null)
                    return null;

                if (result.legend != null && !rhyme.is_ghost)
                    results.Add(result.legend);

                stone = result.stone;
            }

            var legend = results.Count == 1 && (parent == null || parent.returns(results[0].rhyme))
                ? results[0]
                : new Group_Legend(this, results);

            return new Legend_Result(legend, stone);
        }

        override public IEnumerable<Rhyme> aggregate()
        {
            return rhymes;
        }

        protected override List<Rhyme> get_single_type()
        {
            var types = rhymes
                .Where(r => r != this)
                .Select(r => r.vertical_return_types)
                .Where(t => t != null).ToArray();

            var result = new List<Rhyme> { this };
            if (types.Count() == 1)
                result.AddRange(types.First());

            return result;
        }
    }
}
