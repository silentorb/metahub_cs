using System;
using System.Collections.Generic;
using imperative.schema;
using metahub.logic.schema;

namespace metahub.schema
{
public class Namespace
    {
        public string name;
        public string fullname;
        public Dictionary<string, Trellis> trellises = new Dictionary<string, Trellis>();
        //public Function_Library function_library;
        public Dictionary<string, Namespace> children = new Dictionary<string, Namespace>();
        public Namespace parent;
        public Dictionary<string, Region_Additional> additional = new Dictionary<string, Region_Additional>();
        public bool is_external = false;

        public Namespace(string name, string fullname)
        {
            this.name = name;
            this.fullname = fullname;
        }

        public bool has_name(string name)
        {
            return trellises.ContainsKey(name);
        }

        public Namespace get_namespace(List<string> path)
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

    }
}