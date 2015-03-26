using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.Properties;
using runic.lexer;
using runic.parser.rhymes;

namespace runic.parser
{
    public class Parser
    {
        public Dictionary<string, Rhyme> rhymes = new Dictionary<string, Rhyme>();
        public Lexer lexer;
        internal static Lexer_Grammar lexer_grammar = new Lexer_Grammar();
        internal static Parser_Grammar parser_grammar = new Parser_Grammar();

        public Parser()
        {

        }

        public Parser(Lexer lexer, string grammar)
        {
            this.lexer = lexer;
            load_grammar(grammar);
        }

        void load_grammar(string lexicon)
        {
            var runes = Lexer.parser_lexicon.read(lexicon);
            var legend = read(runes, parser_grammar.start);
            process_grammar(legend.children);
        }

        void process_grammar(List<Legend> children)
        {
            foreach (var pattern in children)
            {
                var name = pattern.children[0].text;
                rhymes[name] = create_rhyme(pattern);
            }

            foreach (var pattern in children)
            {
                var name = pattern.children[0].text;
                var rhyme = rhymes[name];
                rhyme.initialize(pattern.children[1], this);
            }
        }

        Rhyme create_rhyme(Legend pattern)
        {
            var name = pattern.children[0].text;
            var group = pattern.children[1];
            var children = group.children;
            switch (group.type)
            {
                case "and":
                    //                    if (children.Count > 1)
                    return new And_Rhyme(name);

                //                    if (group.type == "repetition")
                case "repetition":
                    return new Repetition_Rhyme(name);


                case "or":
                    return new Or_Rhyme(name);

            }
            return new Single_Rhyme(name);

            //            return null;
        }

        public Rhyme get_whisper_rhyme(string name)
        {
            return rhymes.ContainsKey(name)
                ? rhymes[name]
                : new Single_Rhyme(name, lexer.whispers[name]);
        }

        public Rhyme create_child(Legend pattern)
        {
            var text = pattern.text;
            if (pattern.type == "id")
                return get_whisper_rhyme(text);

            if (pattern.type == "repetition")
            {
                var repetition = new Repetition_Rhyme(text);
                repetition.initialize(pattern, this);
                return repetition;
            }

            throw new Exception("Not supported.");
        }

        public Legend read(List<Rune> runes, string start_name = null)
        {
            if (start_name == null)
                start_name = "start";

            var start = rhymes[start_name];
            return read(runes, start);
        }

        public static Legend read(List<Rune> runes, Rhyme start)
        {
            var stone = new Runestone(runes);
            var result = start.match(stone, null);

            if (result != null)
            {
                if (result.stone.is_at_end)
                    return result.legend;

                if (runes.Skip(result.stone.get_position())
                    .All(r => r.whisper.has_attribute(Whisper.Attribute.optional)))
                    return result.legend;
            }

            var furthest = runes[stone.tracker.furthest];
            var last = stone.tracker.history.Last(h => h.success);
            throw new Exception("Could not find match at "
                + furthest.range.end.y + ":" + furthest.range.end.x
                + ", " + furthest.whisper.name + "."
                + "  Last match was " + last.rhyme.name + "."
                );

        }
    }
}
