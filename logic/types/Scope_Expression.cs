using System.Collections.Generic;

namespace metahub.logic.types
{

    public class Scope_Expression : Block
    {
        public Scope scope;

        public Scope_Expression(Scope scope, List<Node> expressions)
            : base(expressions)
        {
            type = Node_Type.scope;
            this.scope = scope;
        }
    }
}