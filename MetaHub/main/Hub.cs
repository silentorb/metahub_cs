

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using metahub.Properties;
using imperative;
using metahub.jackolantern;
using metahub.logic.schema;
using metahub.logic;
using metahub.render;
using metahub.schema;
using parser;
using Regex = System.Text.RegularExpressions.Regex;

namespace metahub
{
    public delegate void Empty_Delegate();

    public class Hub
    {
        public static Regex remove_comments = new Regex("#[^\n]*");
        public Schema root;
        public Schema metahub_schema;
        public int max_steps = 100;
        public Dictionary<string, string> core_schemas = new Dictionary<string, string>(); 

        static List<Hub> instances = new List<Hub>();

        public Hub()
        {
            instances.Add(this);
            root = new Schema();
            

            metahub_schema = root.add_namespace("metahub");

            core_schemas["piecemaker"] = Resources.piecemaker_json;
            core_schemas["metahub"] = Resources.metahub_json;
        }

        public string find_schema(string schema_name, string root_path = null)
        {
            if (root_path != null)
            {
                var path = Path.Combine(root_path, schema_name + ".json").Replace("\\", "/");
                if (File.Exists(path))
                    return File.ReadAllText(path);

            }

            var base_name = Path.GetFileNameWithoutExtension(schema_name);
            if (core_schemas.ContainsKey(base_name))
                return core_schemas[base_name];

            throw new Exception("Could not find schema source for: " + schema_name);
        }

        public void load_schema(string schema_name, string root_path = null)
        {
            var source = find_schema(schema_name, root_path);
            var space = this.root.add_namespace(Path.GetFileNameWithoutExtension(schema_name));
            space.load_from_string(source);
        }

//        public void load_schema_from_file(string url, Schema space, bool auto_identity = false)
//        {
//            var data = JsonConvert.DeserializeObject<Schema_Source>(File.ReadAllText(url));
//            load_schema_from_object(data, space, auto_identity);
//        }
//
//        public void load_schema_from_string(string json, Schema space, bool auto_identity = false)
//        {
//            var data = JsonConvert.DeserializeObject<Schema_Source>(json);
//            load_schema_from_object(data, space, auto_identity);
//        }
//
//        public void load_schema_from_object(Schema_Source data, Schema space, bool auto_identity = false)
//        {
//            root.load_trellises(data.trellises, new Load_Settings(space, auto_identity));
//            if (data.is_external.HasValue && data.is_external.Value)
//                space.is_external = true;
//        }

        public void run_data(Pattern_Source source, Schema railway, Logician logician)
        {
            Coder coder = new Coder(railway, logician);
            coder.convert_statement(source, null);
        }

        public void generate(Pattern_Source source, string target_name, string destination)
        {
            var overlord = new Overlord();
            var logician = new Logician(root);
            run_data(source, root, logician);
            var target = Generator.create_target(overlord, target_name);
            logician.analyze();
            var jack = new JackOLantern(logician, overlord, target);
            jack.run();
            Generator.run(target, destination);
        }
    }
}