using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;

namespace metahub.logic.nodes
{
    public class Lambda : Node
    {
        public List<Parameter> parameters;
        public List<Constraint> constraints;
        public Logic_Scope scope;

        public Lambda(Logic_Scope scope, IEnumerable<Parameter> parameters, IEnumerable<Constraint> constraints)
            : base(Node_Type.lambda)
        {
            this.scope = scope;
            this.parameters = parameters.ToList();
            this.constraints = constraints.ToList();
        }

    }
}