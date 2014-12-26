
using System;
using System.Collections.Generic;
using metahub.schema;
using metahub.schema.Namespace;

/**
 * ...
 * @author Christopher W. Johnson
 */

namespace metahub.logic.schema
{
    /*
    struct Region_Additional
    {
        private bool is_external;
        private string space;
        private string class_export;
    }
     */ 
public class Region
    {
        public Namespace space;
        public bool is_external = false;
        public Dictionary<string, object> trellis_additional = new Dictionary<string, object>();
        public string external_name = null;
        public Dictionary<string, Rail> rails = new Dictionary<string, Rail>();
        public string class_export = "";
        public string name;
        public Region parent = null;
        public Dictionary<string, Region> children = new Dictionary<string, Region>();
        public Dictionary<string, Function_Info> functions = new Dictionary<string, Function_Info>();

        public Region(Namespace space, string target_name)
        {
            this.space =space;
            name = space.name;
            is_external = space.is_external;

            if (space.additional == null)
                return;

            var additional = (Dictionary<string, object>)space.additional[target_name];
            if (additional == null)
                return;

            if (additional.ContainsKey("is_external"))
                is_external = (bool)additional["is_external"];

            if (additional.ContainsKey("namespace"))
                external_name = (string)additional["space"];

            if (additional.ContainsKey("class_export"))
                class_export = (string)additional["class_export"];

            var trellises = (Dictionary<string, object>) additional["trellises"];
            if (trellises != null)
            {
                foreach (var key in trellises.Keys)
                {
                    trellis_additional[key] = trellises[key];
                }
            }
        }

        public void add_functions(List<Function_Info> new_functions)
        {
            foreach (var func in new_functions)
            {
                functions[func.name] = func;
            }
        }

    }
}