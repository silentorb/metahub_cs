using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;
using runic.lexer;

namespace runic.parser
{
    public abstract class Rhyme
    {
        public string name;

        protected Rhyme(string name)
        {
            this.name = name;
        }

        public abstract void initialize(Pattern_Source pattern, Parser parser);
        public abstract Legend_Result match(Runestone stone);
    }
}
