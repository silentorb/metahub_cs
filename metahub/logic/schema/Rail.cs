using System;
using System.Collections.Generic;
using metahub.imperative.schema;
using metahub.schema;

/**
 * ...
 * @author Christopher W. Johnson
 */

namespace metahub.logic.schema
{
//    struct Dictionary<string, object>
//    {
//        private string name,
//        private bool is_external,
//        private string source_file,
//        private string class_export,
//                       object
//        private inserts
//    ,
//        private object default_value
//    }

  public class Rail
    {

        public Trellis trellis;
        public string name;
        public string rail_name;
        public Dictionary<string, Dependency> dependencies = new Dictionary<string, Dependency>();
        public Dictionary<string, Tie> core_ties = new Dictionary<string, Tie>();
        public Dictionary<string, Tie> all_ties = new Dictionary<string, Tie>();
        public Railway railway;
        public Rail parent;
        public bool is_external = false;
        public string source_file = null;
        public Region region;
        public Dictionary<string, object> hooks = new Dictionary<string, object>();
        public List<string> stubs = new List<string>();
        public Dictionary<string, Tie_Addition> property_additional = new Dictionary<string, Tie_Addition>();
        public string class_export = "";
        public object default_value = null;

        public Rail(Trellis trellis, Railway railway)
        {
            this.trellis = trellis;
            this.railway = railway;
            rail_name = this.name = trellis.name;
            region = railway.regions[trellis.space.
            name]
            ;
            is_external = region.is_external;
            class_export = region.class_export;
            load_additional();
            if (!is_external && source_file == null)
                source_file = trellis.space.
            name + "/" + rail_name;
        }

        private void load_additional()
        {
            if (!region.trellis_additional.ContainsKey(trellis.name))
                return;

            Dictionary<string, object> map = region.trellis_additional[trellis.name];

            if (map.ContainsKey("is_external"))
                is_external = (bool)map["is_external"];

            if (map.ContainsKey("name"))
                rail_name = (string)map["name"];

            if (map.ContainsKey("source_file"))
                source_file = (string)map["source_file"];

            if (map.ContainsKey("class_export"))
                class_export = (string)map["class_export"];

            if (map.ContainsKey("default_value")) // Should only be set if is_value is set to true
                default_value = map["default_value"];

            if (map.ContainsKey("hooks"))
            {
                var hook_source = (Dictionary<string, object>) map["hooks"];
                foreach (var key in hook_source.Keys)
                {
                    hooks[key] = hook_source[key];
                }
            }

            if (map.ContainsKey("stubs"))
            {
                var hook_source =(Dictionary<string, string>) map["stubs"];
                foreach (var key in hook_source.Keys)
                {
                    stubs.Add(hook_source[key]);
                }
            }

            if (map.ContainsKey("properties"))
            {
                var properties = (Dictionary<string, object>) map["properties"];
                foreach (var key in properties.Keys)
                {
                    property_additional[key] = properties[key];
                }
            }
        }

        public void process1()
        {
            if (trellis.parent != null)
            {
                parent = railway.get_rail(trellis.parent);
                add_dependency(parent).allow_ambient = false;
            }
            foreach (var property in trellis.properties)
            {
                Tie tie = new Tie(this, property);
                all_ties[tie.name] = tie;
                if (property.trellis == trellis)
                {
                    core_ties[tie.name] = tie;
                    if (property.other_trellis != null)
                    {
                        var dependency = add_dependency(railway.get_rail(property.other_trellis));
                        if (property.type == Kind.list)
                            dependency.allow_ambient = false;
                    }
                }
            }
        }

        public void process2()
        {
            foreach (var tie in all_ties.Values)
            {
                tie.initialize_links();
            }
        }

        private Dependency add_dependency(Rail rail)
        {
            if (!dependencies.ContainsKey(rail.name))
                dependencies[rail.name] = new Dependency(rail);

            return dependencies[rail.name];
        }

        public void finalize()
        {
            foreach (var tie in all_ties.Values)
            {
                tie.finalize();
            }
        }

        public Tie get_tie_or_null(string name)
        {
            if (!all_ties.ContainsKey(name))
                return null;

            return all_ties[name];
        }

        public Tie get_tie_or_error(string name)
        {
            var tie = get_tie_or_null(name);
            if (tie == null)
                throw new Exception("Rail " + this.name + " does not have a tie named " + name + ".");

            return tie;
        }

    }
}