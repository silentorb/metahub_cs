using System.Collections.Generic;
using System.Linq;
using metahub.imperative.types;
using metahub.logic.types;

namespace metahub.logic.schema
{
    public class Constraint
    {
        public static List<string> self_modifying_operators = new List<string>
        {
            "+=",
            "-=",
            "*=",
            "/="
        };

        public Node[] first;
        public Node[] second;
        public bool is_back_referencing = false;
        public string op;
        public List<Constraint> other_constraints = new List<Constraint>();
        public Lambda lambda;
        //public Node[] caller;
        public List<Tie> endpoints;
        public Constraint_Group group;
        public Constraint_Scope constraint_scope;

        public Constraint(Node[] first, Node[] second, string op, Lambda lambda)
        {
            this.op = op;
            this.first = first;
            this.second = second;
            this.lambda = lambda;
            endpoints = get_endpoints(first);

            if (self_modifying_operators.Contains(op))
            {
                var property_node = (Property_Reference) first[0];
                property_node.tie.rail.needs_tick = true;
            }
        }

        public static List<Tie> get_endpoints(Node[] path)
        {
            var i = path.Length;
            while (--i >= 0)
            {
                var token = path[i];
                switch (token.type)
                {
                    case Node_Type.property:
                        var prop = (Property_Reference)token;
                        if (!prop.tie.rail.trellis.is_value)
                            return new List<Tie> { prop.tie };

                        break;

                    case Node_Type.array:
                        var result = new List<Tie>();
                        foreach (var t in ((Array_Expression)token).children)
                        {
                            result.AddRange(get_endpoints(new Node[] { t }));
                        }
                        return result.Distinct().ToList();

                    case Node_Type.path:
                        return get_endpoints(((Reference_Path)token).children.ToArray());
                        break;
                }

            }

            return new List<Tie>();
            //throw new Exception("Could not find endpo inside Node path.");
        }
    }
}