
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using metahub.lab;
using metahub.parser;
using metahub.parser.types;

namespace metahub
{
    class MetaHub_Configuration
    {
        public string[] code;
        public string[] schemas;
        public string output;
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new Exception("Missing configuration path argument.");

            var config_path = args[0].Replace("/", "\\");
            var root = Path.GetDirectoryName(config_path);
            var config = JsonConvert.DeserializeObject<MetaHub_Configuration>(File.ReadAllText(config_path));
            var code = File.ReadAllText(Path.Combine(root, config.code[0]));
            var hub = new metahub.Hub();
            hub.load_parser();
            hub.load_schema("metahub", root);
            foreach (var schema_name in config.schemas)
            {
                hub.load_schema(schema_name, root);
            }

            var result = hub.parse_code(code);
            //Debug_Info.output(result);
            if (!result.success)
                throw new Exception("Syntax Error at " + result.end.y + ":" + result.end.x);

            var match = (Match)result;
            hub.generate(match.get_data(), "cpp", Path.Combine(root, config.output.Replace("/", "\\")));
            Console.WriteLine("done.");
        }

    }

}
/*
var hub = new MetaHub.Hub()
hub.load_parser()

for (var i in config.schemas) {
  var schema_name = config.schemas[i]
  var schema = read_file(root + schema_name + '.json')
  var namespace = hub.schema.add_namespace(path.basename(schema_name, '.json'))
  hub.load_schema_from_string(schema, namespace)
}

var result = hub.parse_code(code)

if (!result.success)
  throw new Error("Syntax Error at " + result.end.y + ":" + result.end.x)

hub.generate(result.get_data(), 'cpp', path.resolve(root, config.output))
*/