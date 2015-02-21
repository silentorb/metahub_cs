

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using metahub.Properties;
using metahub.imperative;
using metahub.jackolantern;
using metahub.logic.schema;
using metahub.logic;
using metahub.render;
using metahub.schema;
using parser;
using Namespace = metahub.schema.Namespace;
using Regex = System.Text.RegularExpressions.Regex;

namespace metahub
{
    public delegate void Empty_Delegate();

    public class Hub
    {
        public static Regex remove_comments = new Regex("#[^\n]*");
        public Schema schema;
        public Definition parser_definition;
        public Namespace metahub_namespace;
        public int max_steps = 100;
        public Dictionary<string, string> core_schemas = new Dictionary<string, string>(); 

        static List<Hub> instances = new List<Hub>();

        public Hub()
        {
            instances.Add(this);
            schema = new Schema();

            metahub_namespace = schema.add_namespace("metahub");

            core_schemas["piecemaker"] = Resources.piecemaker_json;
            core_schemas["metahub"] = Resources.metahub_json;
        }

        public void load_parser()
        {
            Definition boot_definition = new Definition();
            boot_definition.load_parser_schema();
            Bootstrap context = new Bootstrap(boot_definition);

            var result = context.parse(Resources.metahub_grammar,boot_definition.patterns[0], false);
            //Debug_Info.output(result);
            if (result.success)
            {
                var match = (Match)result;

                parser_definition = new Definition();
                parser_definition.load(match.get_data().dictionary);
            }
            else
            {
                throw new Exception("Error loading parser.");
            }
        }

        string find_schema(string schema_name, string root)
        {
            var path = Path.Combine(root, schema_name + ".json").Replace("\\", "/");
            if (File.Exists(path))
                return File.ReadAllText(path);

            var base_name = Path.GetFileNameWithoutExtension(schema_name);
            if (core_schemas.ContainsKey(base_name))
                return core_schemas[base_name];

            throw new Exception("Could not find schema source for: " + schema_name);
        }

        public void load_schema(string schema_name, string root)
        {
            var source = find_schema(schema_name, root);
            var space = schema.add_namespace(Path.GetFileNameWithoutExtension(schema_name));
            load_schema_from_string(source, space);
        }

        public void load_schema_from_file(string url, Namespace space, bool auto_identity = false)
        {
            var data = JsonConvert.DeserializeObject<Schema_Source>(File.ReadAllText(url));
            load_schema_from_object(data, space, auto_identity);
        }

        public void load_schema_from_string(string json, Namespace space, bool auto_identity = false)
        {
            var data = JsonConvert.DeserializeObject<Schema_Source>(json);
            load_schema_from_object(data, space, auto_identity);
        }

        public void load_schema_from_object(Schema_Source data, Namespace space, bool auto_identity = false)
        {
            schema.load_trellises(data.trellises, new Load_Settings(space, auto_identity));
            if (data.is_external.HasValue && data.is_external.Value)
                space.is_external = true;

            if (data.targets != null)
            {
                foreach (var target in data.targets)
                {
                    space.additional[target.Key] = target.Value;
                }
            }
        }

        public void run_data(Pattern_Source source, Railway railway, Logician logician)
        {
            Coder coder = new Coder(railway, logician);
            coder.convert_statement(source, null);
        }

        public Result parse_code(string code)
        {
            if (parser_definition == null)
            {
                load_parser();
            }
            MetaHub_Context context = new MetaHub_Context(parser_definition);
            var without_comments = remove_comments.Replace(code, "");
            //trace("without_comments", without_comments);
            return context.parse(without_comments, parser_definition.patterns[0]);
        }

        //public void load_internal_trellises () {
        //    var functions = Utility.get_string_resource("inserts/core_nodes.json");
        //var data =JsonConvert.DeserializeObject<RootObject>(functions);
        
        //schema.load_trellises(data.trellises, new Load_Settings(metahub_namespace));
        //}

        public void generate(Pattern_Source source, string target_name, string destination)
        {
            Overlord overlord = new Overlord();
            var railway = new Railway(this, target_name);
            var logician = new Logician(railway);
            run_data(source, railway, logician);
            var target = Generator.create_target(overlord, target_name);
            overlord.run(target);
            logician.analyze();
            var jack = new JackOLantern(logician, overlord, this, railway);
            jack.run(target);
            overlord.flatten();
            overlord.post_analyze();

            Generator.run(target, destination);
        }

    }
}