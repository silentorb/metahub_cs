using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;

namespace runic.lexer
{
    public abstract class Whisper
    {
        public enum Attribute
        {
            ignore,
            optional
        }

        public string name;
        public Attribute[] attributes;

        protected Whisper(string name)
        {
            this.name = name;
        }

        public abstract Rune match(string input, int position);

        public bool has_attribute(Attribute attribute)
        {
            return attributes != null && attributes.Contains(attribute);
        }
    }
}
