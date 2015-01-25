using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic.schema;
using metahub.logic.nodes;

namespace metahub.logic
{
    public class Logician
    {
        public List<Constraint> constraints = new List<Constraint>();
        public List<Function_Call2> functions = new List<Function_Call2>(); 
        public Dictionary<string, Constraint_Group> groups = new Dictionary<string, Constraint_Group>();
        public bool needs_hub = false;
        private Railway railway;

        public Logician(Railway railway)
        {
            this.railway = railway;
        }

        public Constraint create_constraint(Node first, Node second, string op, Lambda lambda, Scope scope)
        {
            var constraint = new Constraint(first, second, op, lambda) { constraint_scope = scope.constraint_scope };

            var tie = imperative.code.Parse.get_end_tie(constraint.first);
            if (tie != null)
                tie.constraints.Add(constraint);

            //if (!scope.is_map)
                constraints.Add(constraint);

            return constraint;
        }

        public Function_Call2 call(string name, IEnumerable<Node> inputs, Scope scope = null)
        {
            var result = new Function_Call2(name, inputs);
            functions.Add(result);
            return result;
        }

        public void analyze()
        {
            foreach (var constraint in constraints)
            {
                if (constraint.is_circular)
                    needs_hub = true;
            }
        }
    }
}
