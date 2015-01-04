using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.logic.types
{
   public class Operation_Node : Node
    {
        public string op;
        public Node[] children;

        public Operation_Node(string op, IEnumerable<Node> children)
           : base(Node_Type.operation)
       {
           this.op = op;
           this.children = children.ToArray();
       }
    }
}
