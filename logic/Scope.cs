using System.Collections.Generic;
using metahub.logic.schema;
using metahub.logic.nodes;

namespace metahub.logic
{

    public class Logic_Scope
    {
        public Rail rail;
        public Logic_Scope parent;
        public Dictionary<string, Signature> variables = new Dictionary<string, Signature>();
        public bool is_map = false;
        public Node[] caller;
        public Constraint_Group group;
        public Signature[] parameters;
        public Constraint_Scope constraint_scope;
        public Scope_Node scope_node;
        public Function_Call2 parent_function;

        public Logic_Scope(Logic_Scope parent = null)
        {
            this.parent = parent;
            if (parent != null)
            {
                rail = parent.rail;
                constraint_scope = parent.constraint_scope;
            }
        }

        public Signature find(string name)
        {
            if (variables.ContainsKey(name))
                return variables[name];

            if (parent != null)
                return parent.find(name);

            return null;
        }
    }
}