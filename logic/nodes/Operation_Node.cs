using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.logic.nodes
{
    public class Operation_Node : Node
    {
        public string op;

        public Operation_Node(string op, IEnumerable<Node> children)
            : base(Node_Type.operation)
        {
            this.op = op;
            connect_many_inputs(children);
        }
    }
}
