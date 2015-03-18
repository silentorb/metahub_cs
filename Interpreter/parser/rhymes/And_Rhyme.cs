﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser.rhymes
{
    public class And_Rhyme : Rhyme
    {
        public List<Rhyme> rhymes;

        public And_Rhyme(string name)
            : base(name)
        {

        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {
            rhymes = pattern.patterns.Select(p => parser.create_child(p)).ToList();
        }

        public override Legend_Result match(Runestone stone)
        {
            var results = new List<Legend>();
            foreach (var rhyme in rhymes)
            {
                var result = rhyme.match(stone);
                if (result == null)
                    return null;

                if (result.legend != null)
                    results.Add(result.legend);

                stone = result.stone;
            }

            return new Legend_Result(new Group_Legend(this, results), stone);
        }

        override public IEnumerable<Rhyme> aggregate()
        {
            return rhymes;
        }
    }
}
