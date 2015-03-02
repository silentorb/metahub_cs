using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using metahub.logic;
using vineyard.Properties;
using metahub.logic.schema;
using parser;
using Match = parser.Match;

namespace metahub.schema
{
    public class Schema
    {
        public string name;
        public string fullname;
        public Dictionary<string, Trellis> trellises = new Dictionary<string, Trellis>();
        //public Function_Library function_library;
        public Dictionary<string, Schema> children = new Dictionary<string, Schema>();
        public Schema parent;
        //        public Dictionary<string, Region_Additional> additional = new Dictionary<string, Region_Additional>();
        public bool is_external = false;
        public Dictionary<string, Function_Info> functions = new Dictionary<string, Function_Info>();
        public Schema root;

//        public Schema(string name, string fullname)
//        {
//            this.name = name;
//            this.fullname = fullname;
//            root = this;
//        }

        public Schema()
        {
            name = "";
            root = this;
        }

        public Schema(string name)
        {
            this.name = name;
            fullname = name;
            root = this;
        }

        public bool has_name(string name)
        {
            return trellises.ContainsKey(name);
        }

        public Schema get_namespace(List<string> path)
        {
            var current_namespace = this;
            int i = 0;
            foreach (var token in path)
            {
                if (current_namespace.children.ContainsKey(token))
                {
                    current_namespace = current_namespace.children[token];
                }
                else if (current_namespace.has_name(token) && i == path.Count - 1)
                {
                    return current_namespace;
                }
                else
                {
                    return parent != null
                               ? parent.get_namespace(path)
                               : null;
                }
                ++i;
            }

            return current_namespace;
        }

        public void add_functions(List<Function_Info> new_functions)
        {
            foreach (var func in new_functions)
            {
                functions[func.name] = func;
            }
        }


        public Schema add_namespace(string child_name)
        {
            if (children.ContainsKey(child_name))
                return children[child_name];

            var space = new Schema(child_name)
                {
                    root = root ?? this, 
                    parent = this
                };

            children[child_name] = space;
            return space;
        }

        Trellis add_trellis(string name, Trellis trellis)
        {
            //            trellis.id = trellis_counter++;
            //trellises.Add(trellis);
            return trellis;
        }

        public void load_from_file(string path)
        {
            load_from_string(File.ReadAllText(path));
        }

        public void load_from_string(string json)
        {
            var data = JsonConvert.DeserializeObject<Schema_Source>(json);
            load_from_object(data);
        }

        public void load_from_object(Schema_Source data)
        {
            load_trellises(data.trellises, new Load_Settings(this));
            if (data.is_external.HasValue && data.is_external.Value)
                is_external = true;
        }

        public void load_trellises(Dictionary<string, ITrellis_Source> trellises, Load_Settings settings)
        {
            if (settings.space == null)
                settings.space = this;

            var space = settings.space;
            // Due to cross referencing, loading trellises needs to be done in passes
            // First load the core trellises
            Trellis trellis;
            foreach (var name in trellises.Keys)
            {

                var source = trellises[name];
                if (space.trellises.ContainsKey(name))
                {
                    trellis = space.trellises[name];
                }
                else
                {
                    trellis = space.trellises[name] = add_trellis(name, new Trellis(name, this));
                }

                trellis.load_properties(source);

                if (settings.auto_identity && source.primary_key == null && source.parent == null)
                {
                    var identity_property = trellis.get_property_or_null("id");
                    if (identity_property == null)
                    {
                        identity_property = trellis.add_property("id", new IProperty_Source { type = "int" });
                    }

                    trellis.identity_property = identity_property;
                }

            }

            // Initialize parents
            foreach (var name in trellises.Keys)
            {
                var source = trellises[name];
                trellis = space.trellises[name];
                trellis.initialize1(source, space);
            }

            // Connect everything together
            foreach (var name in trellises.Keys)
            {
                var source = trellises[name];
                trellis = space.trellises[name];
                trellis.initialize2(source);
            }
        }

        public Trellis get_trellis(string trellis_name, bool throw_exception_on_missing = false)
        {
            if (trellis_name.Contains("."))
            {
                var path = trellis_name.Split('.');
//                trellis_name = path.Last();
//                path.RemoveAt(path.Count - 1);
                return resolve_path(path);
            }

            if (!trellises.ContainsKey(trellis_name))
            {
                if (!throw_exception_on_missing)
                    return null;

                throw new Exception("Could not find trellis named: " + trellis_name + ".");
            }
            return trellises[trellis_name];
        }

        public Trellis resolve_path(IEnumerable<string> path)
        {
            var path2 = path.ToList();
            var tokens = path2.Take(path2.Count() - 1);
            var trellis_name = path2.Last();
            var region = root;

            foreach (var token in tokens)
            {
                if (!region.children.ContainsKey(token))
                    throw new Exception("Namespace " + region.name + " does not have space: " + token + ".");

                region = region.children[token];
            }

            if (!region.trellises.ContainsKey(trellis_name))
                throw new Exception("Namespace " + region.name + " does not have a rail named " + trellis_name + ".");

            return region.trellises[trellis_name];
        }

    }
}