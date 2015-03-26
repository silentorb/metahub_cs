using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    public enum Whisper_Type
    {
        regex,
        text,
        group
    }

    public abstract class Whisper
    {
        public enum Attribute
        {
            ignore,
            optional
        }

        public string name;
        public Attribute[] attributes;
        public Whisper_Type type;

        protected Whisper(Whisper_Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public abstract Rune match(string input, Position position);

        public bool has_attribute(Attribute attribute)
        {
            return attributes != null && attributes.Contains(attribute);
        }
    }
}
