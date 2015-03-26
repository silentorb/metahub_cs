using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    public class Lexer_Bootstrap : Lexer
    {
        public Lexer_Bootstrap()
        {
            add_whisper(new Regex_Whisper("spaces", @"[ \t]+") { attributes = new[] { Whisper.Attribute.ignore } });
            add_whisper(new Regex_Whisper("newlines", @"(\s*\n)+\s*") { attributes = new[] { Whisper.Attribute.optional } });
            add_whisper(new Regex_Whisper("string", "\"[^\"]*\"|\\G'[^']*'"));
            add_whisper(new String_Whisper("equals", "="));
            add_whisper(new String_Whisper("or", "|"));
            add_whisper(new String_Whisper("start_group", "("));
            add_whisper(new String_Whisper("end_group", ")"));
            add_whisper(new Regex_Whisper("id", @"[\$a-zA-Z0-9_]+"));
            add_whisper(new Whisper_Group("regex", new List<Whisper>
            {
                new String_Whisper(null, "/"),
                new Regex_Whisper("id", @"[^\/]+"),
                new String_Whisper(null, "/")
            }));
        }
    }
}

//group = "(" trim @(option, option_separator, 2, 0) trim ")"
//option = id | string | regex | group
//comma = /\s*,\s*/
//attribute = /\w+/
//attributes = "(" @(attribute, comma, 0, 0) ")"
//rule = id @(attributes, none, 0, 1) trim "=" trim @(option, option_separator, 0, 0)