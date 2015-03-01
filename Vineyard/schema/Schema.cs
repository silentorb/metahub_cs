using System;
using System.Collections.Generic;
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

        public Schema(string name, string fullname)
        {
            this.name = name;
            this.fullname = fullname;
        }

        public Schema(string name)
        {
            this.name = name;
            fullname = name;
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


        public Schema add_namespace(string name)
        {
            if (children.ContainsKey(name))
                return children[name];

            var space = new Schema(name, name);
            children[name] = space;
            space.parent = this;
            return space;
        }

        Trellis add_trellis(string name, Trellis trellis)
        {
            //            trellis.id = trellis_counter++;
            //trellises.Add(trellis);
            return trellis;
        }

        public void load_from_string(string json)
        {
            var data = JsonConvert.DeserializeObject<Schema_Source>(json);
            load_from_object(data);
        }

        public void load_from_object(Schema_Source data)
        {
            load_trellises(data.trellises, new Load_Settings(this));
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
                var path = trellis_name.Split('.').ToList();
                trellis_name = path.Last();
                path.RemoveAt(path.Count - 1);
            }

            if (!trellises.ContainsKey(trellis_name))
            {
                if (!throw_exception_on_missing)
                    return null;

                throw new Exception("Could not find trellis named: " + trellis_name + ".");
            }
            return trellises[trellis_name];
        }


        public Trellis resolve_rail_path(IEnumerable<string> path)
        {
            var tokens = path.Take(path.Count() - 1);
            var rail_name = path.Last();
            var region = this;
            foreach (var token in tokens)
            {
                if (!region.children.ContainsKey(token))
                    throw new Exception("Namespace " + region.name + " does not have space: " + token + ".");

                region = region.children[token];
            }

            if (!region.trellises.ContainsKey(rail_name))
                throw new Exception("Namespace " + region.name + " does not have a rail named " + rail_name + ".");

            return region.trellises[rail_name];
        }

    }
}