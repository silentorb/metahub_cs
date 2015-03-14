using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;

namespace runic.lexer
{
    public abstract class Whisper
    {
        public string name;

        protected Whisper(string name)
        {
            this.name = name;
        }

        public abstract Rune match(string input, int position);
        
        public static Whisper create(string name, Pattern_Source source)
        {
            switch (source.type)
            {
                case "regex":
                    return new Regex_Whisper(name, source.text);

                case "literal":
                    return new String_Whisper(name, source.text);

                case "or":
                    return new Whisper_Group(name, source.patterns.Select(s => create(s.text, s)));
            }

            throw new Exception("Unknown whisper type: " + source.type + ".");
        }
    }
}
