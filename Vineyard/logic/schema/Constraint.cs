using System.Collections.Generic;
using System.Linq;
using metahub.logic.nodes;
using metahub.schema;

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
        public List<Property> endpoints;
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
                property_node.tie.trellis.needs_tick = true;
            }
        }

        public static List<Property> get_endpoints(Node node)
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
                        if (!prop.tie.trellis.is_value)
                            return new List<Property> { prop.tie };

                        break;

                    case Node_Type.array:
                        var result = new List<Property>();
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

            return new List<Property>();
            //throw new Exception("Could not find endpo inside Node path.");
        }
    }
}