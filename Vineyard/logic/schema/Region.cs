
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
        public Schema space;
        public bool is_external = false;
        public string external_name = null;
        public Dictionary<string, Trellis> trellises = new Dictionary<string, Trellis>();
        public string class_export = "";
        public string name;
        public Schema parent = null;
        public Dictionary<string, Schema> children = new Dictionary<string, Schema>();

        public Namespace2(Schema space, string target_name)
        {
            this.space =space;
            name = space.name;
            is_external = space.is_external;
        }


    }
}