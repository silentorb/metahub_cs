using System.Collections.Generic;

namespace metahub.logic.nodes
{

    public class Scope_Expression : Block
    {
        public Logic_Scope scope;

        public Scope_Expression(Logic_Scope scope, List<Node> expressions)
            : base(expressions)
        {
            type = Node_Type.scope;
            this.scope = scope;
        }
    }
}