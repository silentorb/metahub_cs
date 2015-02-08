using System.Collections.Generic;
using System.Linq;
using metahub.imperative.types;
using metahub.logic.nodes;

namespace metahub.logic.schema
{
    public class Constraint
    {
        public static List<string> circular_operators = new List<string>
        {
            "+=",
            "-=",
            "*=",
            "/="
        };

        public Node first;
        public Node second;
        public bool is_circular = false;
        public string op;
        public List<Constraint> other_constraints = new List<Constraint>();
        public Lambda lambda;
        //public Node[] caller;
        public List<Tie> endpoints;
        public Constraint_Group group;
        public Constraint_Scope constraint_scope;

        public Constraint(Node first, Node second, string op, Lambda lambda)
        {
            this.op = op;
            this.first = first;
            this.second = second;
            this.lambda = lambda;
            endpoints = get_endpoints(first).Concat(get_endpoints(second)).Distinct().ToList();

            if (circular_operators.Contains(op))
            {
                is_circular = true;
                var property_node = (Property_Node) first;
                property_node.tie.rail.needs_tick = true;
            }
        }

        public static List<Tie> get_endpoints(Node node)
        {
            var path = node.get_path();
            var i = path.Count;
            while (--i >= 0)
            {
                var token = path[i];
                switch (token.type)
                {
                    case Node_Type.property:
                        var prop = (Property_Node)token;
                        if (!prop.tie.rail.trellis.is_value)
                            return new List<Tie> { prop.tie };

                        break;

                    case Node_Type.array:
                        var result = new List<Tie>();
                        foreach (var t in ((Array_Expression)token).children)
                        {
                            result.AddRange(get_endpoints(t));
                        }
                        return result.Distinct().ToList();

                    //case Node_Type.path:
                    //    return get_endpoints(token);
                    //    break;
                }

            }

            return new List<Tie>();
            //throw new Exception("Could not find endpo inside Node path.");
        }
    }
}