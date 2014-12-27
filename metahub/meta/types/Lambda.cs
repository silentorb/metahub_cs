using System.Collections.Generic;
using System.Linq;

namespace metahub.meta.types
{
    public class Lambda : Node
    {
        public List<Parameter> parameters;
        public List<Node> expressions;
        public Scope scope;

        public Lambda(Scope scope, IEnumerable<Parameter> parameters, IEnumerable<Node> expressions)
            : base(Node_Type.lambda)
        {
            this.scope = scope;
            this.parameters = parameters.ToList();
            this.expressions = expressions.ToList();
        }

    }
}