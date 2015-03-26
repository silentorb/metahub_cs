using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;
using runic.parser.rhymes;

namespace runic.parser
{
    class Parser_Grammar
    {
        public Rhyme start;

        public Parser_Grammar()
        {
            var lexer = Lexer.parser_lexicon;

            var repetition = new And_Rhyme("repetition", new List<Rhyme>
            {
                new Single_Rhyme(lexer.start_rep),
                new Repetition_Rhyme(new Single_Rhyme(lexer.id), new Single_Rhyme(lexer.comma), 3, 4),
                new Single_Rhyme(lexer.end_rep)
            });

            var option = new Or_Rhyme("option", new List<Rhyme>
            {
                new Single_Rhyme(lexer.id),
                repetition
            });

            var group = new Or_Rhyme("group", new List<Rhyme>
            {
                new Repetition_Rhyme(option, new Single_Rhyme(lexer.or), 2, 0) { name = "or" },
                new Repetition_Rhyme(option, new Single_Rhyme(lexer.spaces), 2, 0) { name = "and" },
                option
            });

            var rule = new And_Rhyme("rule", new List<Rhyme>
            {
                new Single_Rhyme(lexer.id),
                new Single_Rhyme(lexer.equals),
                group
            });

            start = new Repetition_Rhyme(rule, new Single_Rhyme(lexer.newlines), 1, 0);
        }
    }
}

// repetition = "@(" trim @(id, comma, 3, 4) trim ")"
// option = id | repetition
// group = @(option, or, 2, 0) | @(option, spaces, 0, 0)
// rule = id trim "=" trim group
