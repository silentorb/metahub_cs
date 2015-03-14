using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using parser;
using runic.lexer;
using Match = parser.Match;

namespace interpreter.runic
{
    public class Lexer
    {
        private Definition definition;
        public Whisper[] whispers;

        public Lexer()
        {
        }

        public Lexer(string lexicon)
        {
            load_lexicon(lexicon);
        }

        void load_lexicon(string lexicon)
        {
            Definition boot_definition = new Definition();
            boot_definition.load_parser_schema();
            Bootstrap context = new Bootstrap(boot_definition);

            var result = context.parse(lexicon, boot_definition.patterns[0], false);
            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Error loading parser.");
            }

            var match = (Match)result;
            process_lexicon(match.get_data().dictionary);
        }

        void process_lexicon(Dictionary<string, Pattern_Source> dictionary)
        {
            whispers = dictionary.Select(s => Whisper.create(s.Key, s.Value)).ToArray();
        }

        public List<Rune> read(string input)
        {
            var result = new List<Rune>();

            int position = 0;
            while (position < input.Length)
            {
                foreach (var whisper in whispers)
                {
                    var rune = whisper.match(input, position);
                    if (rune != null)
                    {
                        result.Add(rune);
                        position += rune.length;
                        break;
                    }
                }
            }

            return result;
        }

        public static string load_resource(string filename)
        {
            var path = "runic.resources." + filename;
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                throw new Exception("Could not find file " + path + ".");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd().Replace("\r\n", "\n");
        }
    }
}
