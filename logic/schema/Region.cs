
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
    
public class Namespace2
    {
        public Namespace space;
        public bool is_external = false;
        public string external_name = null;
        public Dictionary<string, Trellis> trellises = new Dictionary<string, Trellis>();
        public string class_export = "";
        public string name;
        public Namespace parent = null;
        public Dictionary<string, Namespace> children = new Dictionary<string, Namespace>();

        public Namespace2(Namespace space, string target_name)
        {
            this.space =space;
            name = space.name;
            is_external = space.is_external;
        }


    }
}