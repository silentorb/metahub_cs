
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
    
public class Region
    {
        public Namespace space;
        public bool is_external = false;
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