using System.Collections.Generic;
using System.Linq;

namespace metahub.logic.nodes
{
    public class Lambda : Node
    {
        public Logic_Scope scope;
        public string[] parameter_names;

        public Lambda(Logic_Scope scope, IEnumerable<Parameter_Node> parameters)
            : base(Node_Type.lambda)
        {
            this.scope = scope;
            connect_many_inputs(parameters);
            this.parameter_names = parameters.Select(p => p.name).ToArray();
        }

        public override Node clone()
        {
            return new Lambda(scope, inputs.OfType<Parameter_Node>());
        }
    }
}