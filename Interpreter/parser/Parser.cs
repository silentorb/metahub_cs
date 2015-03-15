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

        public Rhyme create_child(Pattern_Source pattern)
        {
            return null;
        }

        public void read(List<Rune> runes)
        {

        }
    }
}
