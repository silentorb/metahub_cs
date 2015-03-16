using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser
{
    public class Legend_Result
    {
        public Legend legend;
        public Runestone stone;

        public Legend_Result(Legend legend, Runestone stone)
        {
            this.legend = legend;
            this.stone = stone;
        }
    }

    public class Legend
    {
        public List<Legend> children = new List<Legend>();
        public string text;
        public Rhyme rhyme;

        public Legend(Rhyme rhyme, string text)
        {
            this.rhyme = rhyme;
            this.text = text;
        }
    }
}
