using System.Collections.Generic;

namespace metahub.meta.types
{
    public class Block : Node
    {
        public List<Node> children = new List<Node>();

        public Block(List<Node> statements = null)
            : base(Node_Type.block)
        {
            if (statements != null)
                children = statements;

        }
    }
}