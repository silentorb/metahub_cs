using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;

namespace metahub.logic.types
{
    public class Lambda : Node
    {
        public List<Parameter> parameters;
        public List<Constraint> constraints;
        public Scope scope;

        public Lambda(Scope scope, IEnumerable<Parameter> parameters, IEnumerable<Constraint> constraints)
            : base(Node_Type.lambda)
        {
            this.scope = scope;
            this.parameters = parameters.ToList();
            this.constraints = constraints.ToList();
        }

    }
}