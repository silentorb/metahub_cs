using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using metahub.logic.schema;

namespace metahub.schema
{

    [DebuggerDisplay("{name} Trellis")]
    public class Trellis2
    {
        public Trellis trellis;
        public string name;
        public string rail_name;
        public Dictionary<string, Property> core_properties = new Dictionary<string, Property>();
        public Dictionary<string, Property> all_properties = new Dictionary<string, Property>();
        public Railway2 railway;
        public Trellis parent;
//        public bool is_external = false;
//        public string source_file = null;
        public Namespace space;
//        public Dictionary<string, object> hooks = new Dictionary<string, object>();
//        public List<string> stubs = new List<string>();
//        public Dictionary<string, Tie_Addition> property_additional = new Dictionary<string, Tie_Addition>();
//        public string class_export = "";
        public object default_value = null;
        public bool needs_tick = false;

        public Trellis2(Trellis trellis, Railway2 railway)
        {
            this.trellis = trellis;
            this.railway = railway;
            rail_name = name = trellis.name;
            space = railway.regions[trellis.space.name];
        }

//        public void process1()
//        {
//            if (trellis.parent != null && !trellis.parent.is_abstract)
//            {
//                parent = railway.get_rail(trellis.parent);
//                //add_dependency(parent).allow_partial = false;
//            }
//
//            foreach (var property in trellis.properties)
//            {
//                Property tie = new Property(this, property);
//                all_properties[tie.name] = tie;
//                if (property.trellis == trellis)
//                {
//                    core_properties[tie.name] = tie;
//                }
//            }
//        }

        public void process2()
        {
            foreach (var tie in all_properties.Values)
            {
//                tie.initialize_links();
            }
        }

        public void finalize()
        {
            foreach (var tie in all_properties.Values)
            {
//                tie.finalize();
            }
        }

        public Property get_tie_or_null(string name)
        {
            if (!all_properties.ContainsKey(name))
                return null;

            return all_properties[name];
        }

        public Property get_tie_or_error(string name)
        {
            var tie = get_tie_or_null(name);
            if (tie == null)
                throw new Exception("Trellis " + this.name + " does not have a tie named " + name + ".");

            return tie;
        }

        public Property get_reference(Trellis rail)
        {
            return all_properties.Values.FirstOrDefault(t => t.other_trellis == rail);
        }
    }
}