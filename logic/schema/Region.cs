
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using metahub.schema;

/**
 * ...
 * @author Christopher W. Johnson
 */

namespace metahub.logic.schema
{
    
    public class Region_Additional
    {
        public bool? is_external;

        [JsonProperty("namespace")]
        public string space;
        public string class_export;
        public Dictionary<string, Rail_Additional> trellises;
    }
      
public class Region
    {
        public Namespace space;
        public bool is_external = false;
        public Dictionary<string, Rail_Additional> trellis_additional = new Dictionary<string, Rail_Additional>();
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

            if (!space.additional.ContainsKey(target_name))
                return;

            var additional = space.additional[target_name];

            if (additional.is_external.HasValue)
                is_external = additional.is_external.Value;

            if (additional.space != null)
                external_name = additional.space;

            if (additional.class_export != null)
                class_export = additional.class_export;

            if (additional.trellises != null)
            {
                foreach (var item in additional.trellises)
                {
                    trellis_additional[item.Key] = item.Value;
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