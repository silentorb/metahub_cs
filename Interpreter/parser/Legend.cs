using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser
{

    public abstract class Legend
    {
        public Rhyme rhyme;
    }

    public class String_Legend : Legend
    {
        public string text;

        public String_Legend(Rhyme rhyme, string text)
        {
            this.rhyme = rhyme;
            this.text = text;
        }
    }

    public class Group_Legend : Legend
    {
        public List<Legend> children;
        public List<Legend> dividers;

        public Group_Legend(Rhyme rhyme, List<Legend> children, List<Legend> dividers = null)
        {
            this.rhyme = rhyme;
            this.children = children;
            this.dividers = dividers;
        }
    }
}
