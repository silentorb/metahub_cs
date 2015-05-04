

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using metahub.Properties;
using imperative;
using imperative.schema;
using metahub.jackolantern;
using metahub.logic;
using metahub.render;
using metahub.schema;
using parser;
using Regex = System.Text.RegularExpressions.Regex;

namespace metahub.main
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

        public void load_schema_files(MetaHub_Configuration config)
        {

            foreach (var schema_name in config.schemas)
            {
                var schema = root.add_namespace(schema_name);
//                schema.load_from_string(find_schema(schema_name, root_path));
            }

        }

        public static void run(Overlord_Configuration config)
        {
            var hub = new Hub();
            hub.load_schema("metahub");

            //hub.load_schema_files(config);
            
            var files = Overlord.aggregate_files(config.input);
            var overlord = new Overlord(config.target);
            var logician = new Logician(hub.root);

            var imp_files = files.Where(f => Path.GetExtension(f) == ".imp");
            var metahub_files = files.Where(f => Path.GetExtension(f) == ".mh");

            overlord.summon_many(imp_files);
            var jack = new JackOLantern(logician, overlord, overlord.target);
            dungeons_to_trellises(overlord.root, logician.schema, jack);

            foreach (var clan in jack.clans.Values)
            {
                foreach (var portal in clan.dungeon.all_portals.Values)
                {
                    var other_trellis = portal.other_dungeon != null
                        ? jack.clans[(Dungeon) portal.other_dungeon].trellis
                        : null;
                    clan.trellis.add_property(new Property(portal.name, portal.type, clan.trellis, other_trellis));
                }
            }

            hub.load_many(metahub_files, logician);

            logician.analyze();
            jack.run();

            overlord.generate(config);
        }

        void load_many(IEnumerable<string> paths, Logician logician)
        {
            foreach (var file in paths)
            {
                load_file(file, logician);
            }
        }

        void load_file(string path, Logician logician)
        {
            var result = logician.parse_code(File.ReadAllText(path));
            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Syntax Error at " + result.end.y + ":" + result.end.x);
            }
            var match = (Match)result;

            run_data(match.get_data(), root, logician);
        }

        static void dungeons_to_trellises(Realm realm, Schema schema, JackOLantern jack)
        {
            foreach (var dungeon in realm.dungeons.Values)
            {
                dungeon_to_trellis(dungeon, schema, jack);
            }

            foreach (var child in realm.children.Values)
            {
                var child_schema = schema.get_namespace(new List<string> {child.name })
                    ?? schema.add_namespace(child.name);
                
                dungeons_to_trellises(child, child_schema, jack);
            }
        }

        static void dungeon_to_trellis(Dungeon dungeon, Schema schema, JackOLantern jack)
        {
            if (schema.trellises.ContainsKey(dungeon.name))
                return;

            var trellis = new Trellis(dungeon.name, schema);
            jack.add_clan(dungeon, trellis);
        }
    }
}