using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using parser;
using runic.lexer;

namespace runic.parser
{
    [DebuggerDisplay("Rhyme {name}")]
    public abstract class Rhyme
    {
        public string name;

        protected Rhyme(string name)
        {
            this.name = name;
        }

        public abstract void initialize(Pattern_Source pattern, Parser parser);
        public abstract Legend_Result match(Runestone stone);
        public abstract IEnumerable<Rhyme> aggregate();
    }
}
