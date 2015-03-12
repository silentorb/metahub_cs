using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using parser;

namespace interpreter.runic
{
    public class Lexer
    {
        private Definition definition;

        public Lexer()
        {
            Definition boot_definition = new Definition();
            boot_definition.load_parser_schema();
            Bootstrap context = new Bootstrap(boot_definition);

            var result = context.parse(load_resource("lexer.grammar"), boot_definition.patterns[0], false);

            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Could not load grammar.");
            }

            var match = (Match)result;

            definition = new Definition();
            definition.load(match.get_data().dictionary);
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
