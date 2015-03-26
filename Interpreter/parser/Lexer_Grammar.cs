using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;
using runic.parser.rhymes;

namespace runic.parser
{
    class Lexer_Grammar
    {
        public Rhyme start;

        public Lexer_Grammar()
        {
            var lexer = Lexer.lexer_lexicon;

            var option = new Or_Rhyme("option", new List<Rhyme>
            {
                new Single_Rhyme(lexer.id),
                new Single_Rhyme(lexer.string_value),
                new Single_Rhyme(lexer.regex)
            });

//            var repetition = new Repetition_Rhyme(option, 2, 0);
//            option.rhymes.Add(repetition);
            var attributes = new And_Rhyme("attributes", new List<Rhyme>
            {
                new Single_Rhyme(lexer.start_group),
                new Repetition_Rhyme(new Single_Rhyme(lexer.id), new Single_Rhyme(lexer.comma), 0, 0),
                new Single_Rhyme(lexer.end_group)
            });

            var rule = new And_Rhyme("rule", new List<Rhyme>
            {
                new Single_Rhyme(lexer.id),
                new Repetition_Rhyme(attributes, 0, 1),
                new Single_Rhyme(lexer.equals),
                new Repetition_Rhyme(option, new Single_Rhyme(lexer.or), 1, 0)
            });

            start = new Repetition_Rhyme(rule, new Single_Rhyme(lexer.newlines),  1, 0);
        }
    }
}

// group = "(" trim @(option, option_separator, 2, 0) trim ")"
// option = id | string | regex | group
// attributes = "(" @(attribute, comma, 0, 0) ")"
// rule = id @(attributes, none, 0, 1) trim "=" trim @(option, option_separator, 0, 0)