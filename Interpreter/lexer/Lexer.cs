using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using parser;
using runic.Properties;
using Match = parser.Match;

namespace runic.lexer
{
    public class Lexer
    {
        public Dictionary<string, Whisper> whispers = new Dictionary<string, Whisper>();
        public static Lexer_Bootstrap boot = new Lexer_Bootstrap();

        public Lexer()
        {
        }

        public Lexer(string lexicon)
        {
            load_lexicon(lexicon);
        }

        void load_lexicon(string lexicon)
        {
            var runes = boot.read(lexicon);
//            Lexer_Bootstrap_Old context = new Lexer_Bootstrap_Old(definition);
//
//            var result = context.parse(lexicon, definition.patterns[0], false);
//            if (!result.success)
//            {
//                Debug_Info.output(result);
//                throw new Exception("Error loading parser.");
//            }
//
//            var match = (Match)result;
//            var data = match.get_data();
//            process_lexicon(data.patterns[1].patterns);
        }

        Whisper add_whisper(string name, Whisper whisper)
        {
            if (whispers.ContainsKey(name))
                throw new Exception("Lexer already contains a whisper named " + name + ".");

            whispers[name] = whisper;
            return whisper;
        }

        protected Whisper add_whisper(Whisper whisper)
        {
            if (whispers.ContainsKey(whisper.name))
                throw new Exception("Lexer already contains a whisper named " + whisper.name + ".");

            whispers[whisper.name] = whisper;
            return whisper;
        }

        void process_lexicon(Pattern_Source[] patterns)
        {
            foreach (var pattern in patterns)
            {
                var name = pattern.patterns[0].text;
                add_whisper(name, create_whisper(pattern));
            }

            foreach (var pattern in patterns.Where(p => p.patterns[5].patterns.Length > 1))
            {
                var name = pattern.patterns[0].text;
                var whisper = (Whisper_Group)whispers[name];
                whisper.whispers = pattern.patterns[5].patterns.Select(p =>
                    {
                        var child = create_sub_whisper(p.text, p);
                        if (p.type == "string" || p.type == "regex")
                            add_whisper(child.name, child);

                        return child;
                    }).ToArray();
            }
        }

        Whisper create_whisper(Pattern_Source source)
        {
            var name = source.patterns[0].text;
            if (source.patterns.Length < 6)
            {

            }
            var patterns = source.patterns[5].patterns;

            var whisper = patterns.Length > 1
                ? new Whisper_Group(name)
                : create_sub_whisper(name, patterns[0]);

            var attributes = source.patterns[1].patterns;
            if (attributes.Length > 0)
            {
                whisper.attributes = attributes[0].patterns[1].patterns.Select(p =>
                    {
                        Whisper.Attribute result;
                        Enum.TryParse(p.text, out result);
                        return result;
                    }).ToArray();
            }
            return whisper;
        }

        Whisper create_sub_whisper(string name, Pattern_Source source)
        {
            switch (source.type)
            {
                case "regex":
                    return new Regex_Whisper(name, source.text);

                case "string":
                    return new String_Whisper(name, source.text);

            }

            var text = source.text;
            if (text != null)
            {
                if (whispers.ContainsKey(text))
                    return whispers[text];
            }

            throw new Exception("Unknown whisper type: " + source.type + ".");
        }

        public List<Rune> read(string input)
        {
            var result = new List<Rune>();

            int index = 0;
            var position = new Position(input);

            while (index < input.Length)
            {
                var rune = next(input, position);
                if (rune == null)
                    throw new Exception("Could not find match at " + index + " " + get_safe_substring(input, index, 10));

                if (rune.length == 0)
                    throw new Exception("Invalid Whisper:" + rune.whisper.name);

                if (!rune.whisper.has_attribute(Whisper.Attribute.ignore))
                    result.Add(rune);

                index += rune.length;
            }

            return result;
        }

        Rune next(string input, Position position)
        {
            foreach (var whisper in whispers.Values)
            {
                var rune = whisper.match(input, position);
                if (rune != null)
                    return rune;
            }

            return null;
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

        public static string get_safe_substring(string text, int start)
        {
            return start >= text.Length ? "" : text.Substring(start);
        }

        public static string get_safe_substring(string text, int start, int end)
        {
            if (start >= text.Length)
                return "";

            if (start + end >= text.Length)
                return text.Substring(start);

            return text.Substring(start, end);
        }
    }
}
