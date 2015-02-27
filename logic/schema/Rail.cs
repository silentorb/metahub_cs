using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using imperative.schema;
using metahub.schema;

namespace metahub.logic.schema
{
    [DebuggerDisplay("{name} Rail")]
    public class Rail
    {

        public Trellis trellis;
        public string name;
        public string rail_name;
        public Dictionary<string, Tie> core_ties = new Dictionary<string, Tie>();
        public Dictionary<string, Tie> all_ties = new Dictionary<string, Tie>();
        public Railway railway;
        public Rail parent;
//        public bool is_external = false;
//        public string source_file = null;
        public Region region;
//        public Dictionary<string, object> hooks = new Dictionary<string, object>();
//        public List<string> stubs = new List<string>();
//        public Dictionary<string, Tie_Addition> property_additional = new Dictionary<string, Tie_Addition>();
//        public string class_export = "";
        public object default_value = null;
        public bool needs_tick = false;

        public Rail(Trellis trellis, Railway railway)
        {
            this.trellis = trellis;
            this.railway = railway;
            rail_name = name = trellis.name;
            region = railway.regions[trellis.space.name];
        }

        public void process1()
        {
            if (trellis.parent != null && !trellis.parent.is_abstract)
            {
                parent = railway.get_rail(trellis.parent);
                //add_dependency(parent).allow_partial = false;
            }

            foreach (var property in trellis.properties)
            {
                Tie tie = new Tie(this, property);
                all_ties[tie.name] = tie;
                if (property.trellis == trellis)
                {
                    core_ties[tie.name] = tie;
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

        public Tie get_reference(Rail rail)
        {
            return all_ties.Values.FirstOrDefault(t => t.other_rail == rail);
        }
    }
}