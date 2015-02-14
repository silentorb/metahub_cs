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
        public Constraint_Group group;
        public Signature[] parameters;
        public Constraint_Scope constraint_scope;
        public Node scope_node;
        public Node bounce_node;
        public Function_Node parent_function;

        public Logic_Scope(Logic_Scope parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
                rail = parent.rail;
                constraint_scope = parent.constraint_scope;
                scope_node = parent.scope_node;
                parent_function = parent.parent_function;
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