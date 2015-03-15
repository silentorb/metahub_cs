using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;
using runic.Properties;
using runic.lexer;

namespace runic.parser
{
    public class Parser
    {
        
        public Parser()
        {
        }

        public Parser(string lexicon)
        {
            load_grammar(lexicon);
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
            Bootstrap_Legacy context = new Bootstrap_Legacy(definition);

            var result = context.parse(lexicon, definition.patterns[0], false);
            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Error loading parser.");
            }

            var match = (Match)result;
            var data = match.get_data();
//            process_lexicon(data.patterns[1].patterns);
        }
        public void read(List<Rune> runes)
        {
            
        }
    }
}
