using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using metahub.logic.nodes;

namespace vineyard.transform
{
    [DebuggerDisplay("Node_Link {node.debug_string} {dir}")]
    public class Node_Link
    {
        public Node node;
        public Dir dir;
        public Node previous;

        public Node_Link(Node node, Dir dir, Node previous)
        {
            this.node = node;
            this.dir = dir;
            this.previous = previous;
        }
    }

}
