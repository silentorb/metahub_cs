using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;
using runic.Properties;
using runic.lexer;
using runic.parser.rhymes;

namespace runic.parser
{
    public class Parser
    {
        public Dictionary<string, Rhyme> rhymes = new Dictionary<string, Rhyme>();
        public Lexer lexer;

        public Parser(Lexer lexer, string grammar)
        {
            this.lexer = lexer;
            load_grammar(grammar);
        }

        static Definition bootstrap()
        {
            Definition boot_definition = new Definition();
            boot_definition.load_parser_schema();
            Bootstrap context = new Bootstrap(boot_definition);

            var result = context.parse(Resources.parser_grammar, boot_definition.patterns[0], false);
            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Error loading parser.");
            }

            var match = (Match)result;
            var definition = new Definition();
            definition.load(match.get_data().dictionary);
            return definition;
        }

        void load_grammar(string lexicon)
        {
            var definition = bootstrap();
            var context = new Parser_Bootstrap(definition);

            var result = context.parse(lexicon, definition.patterns[0], false);
            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Error loading parser.");
            }

            var match = (Match)result;
            var data = match.get_data();
            process_grammar(data.patterns[1].patterns);
        }

        void process_grammar(Pattern_Source[] patterns)
        {
            foreach (var pattern in patterns)
            {
                var name = pattern.patterns[0].text;
                rhymes[name] = create_rhyme(pattern);
            }

            foreach (var pattern in patterns)
            {
                var name = pattern.patterns[0].text;
                var rhyme = rhymes[name];
                rhyme.initialize(pattern.patterns[4], this);
            }
        }

        Rhyme create_rhyme(Pattern_Source pattern)
        {
            var name = pattern.patterns[0].text;
            var group = pattern.patterns[4];
            var patterns = group.patterns;
            switch (group.type)
            {
                case "and":
                    if (patterns.Length > 1)
                        return new And_Rhyme(name);

                    if (patterns[0].type == "repetition")
                        return new Repetition_Rhyme(name);

                    return new Single_Rhyme(name);

                case "or":
                    return new Or_Rhyme(name);

            }

            return null;
        }

        public Rhyme get_whisper_rhyme(string name)
        {
            return rhymes.ContainsKey(name)
                ? rhymes[name]
                : new Single_Rhyme(name, lexer.whispers[name]);
        }

        public Rhyme create_child(Pattern_Source pattern)
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

            var stone = new Runestone(runes);
            var result = start.match(stone, null);

            if (result == null || !result.stone.is_at_end)
            {
                var furthest = runes[stone.tracker.furthest];
                var last = stone.tracker.history.Last(h => h.success);
                throw new Exception("Could not find match at "
                    + furthest.range.end.y + ":" + furthest.range.end.x
                    + ", " + furthest.whisper.name + "."
                    + "  Last match was " + last.rhyme.name + "."
                    );
            }
            return result.legend;
        }
    }
}
