using System.Collections.Generic;
using System.Linq;

namespace metahub.logic.nodes
{
    public class Lambda : Node
    {
        public Logic_Scope scope;

        public Lambda(Logic_Scope scope, IEnumerable<Parameter_Node> parameters)
            : base(Node_Type.lambda)
        {
            this.scope = scope;
            connect_many_inputs(parameters);
        }

    }
}