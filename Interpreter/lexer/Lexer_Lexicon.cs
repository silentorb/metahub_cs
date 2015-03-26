using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    public class Lexer_Lexicon : Lexer
    {
        public Regex_Whisper id = new Regex_Whisper("id", @"[\$a-zA-Z0-9_]+");
        public String_Whisper or = new String_Whisper("or", "|");
        public Regex_Whisper string_value = new Regex_Whisper("string", "\"([^\"]*)\"|\\G'([^']*)'");
        public String_Whisper start_group = new String_Whisper("start_group", "(");
        public String_Whisper end_group = new String_Whisper("end_group", ")");
        public String_Whisper equals = new String_Whisper("equals", "=");
        public String_Whisper comma = new String_Whisper("comma", ",");
        public Regex_Whisper regex = new Regex_Whisper("regex", @"/([^/]+)/");
        public Regex_Whisper newlines = new Regex_Whisper("newlines", @"(\s*\n)+\s*")
        {
            attributes = new[] { Whisper.Attribute.optional }
        };
//        public String_Whisper forward_slash = new String_Whisper("forward_slash", "/");


        public Lexer_Lexicon()
        {
            add_whisper(new Regex_Whisper("spaces", @"[ \t]+") { attributes = new[] { Whisper.Attribute.ignore } });
            add_whisper(newlines);
            add_whisper(string_value);
            add_whisper(equals);
            add_whisper(or);
            add_whisper(comma);
            add_whisper(start_group);
            add_whisper(end_group);
            add_whisper(regex);
            add_whisper(id);
        }
    }
}
