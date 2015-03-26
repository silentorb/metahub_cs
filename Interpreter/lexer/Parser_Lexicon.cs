using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    public class Parser_Lexicon : Lexer
    {
        public Parser_Lexicon()
        {
            add_whisper(new Regex_Whisper("spaces", @"[ \t]+") { attributes = new[] { Whisper.Attribute.ignore } });
            add_whisper(new Regex_Whisper("newlines", @"(\s*\n)+\s*") { attributes = new[] { Whisper.Attribute.optional } });
            add_whisper(new String_Whisper("equals", "="));
            add_whisper(new String_Whisper("or", "|"));
            add_whisper(new String_Whisper("comma", ","));
            add_whisper(new String_Whisper("start_rep", "@("));
            add_whisper(new String_Whisper("end_rep", ")"));
            add_whisper(new Regex_Whisper("integer", @"\d+"));
            add_whisper(new Regex_Whisper("id", @"[\$a-zA-Z0-9_]+"));
        }
    }
}
