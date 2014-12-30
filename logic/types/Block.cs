using System.Collections.Generic;
using System.Linq;

namespace metahub.logic.types
{
    public class Block : Node
    {
        public List<Node> children = new List<Node>();

        public Block(IEnumerable<Node> statements = null)
            : base(Node_Type.block)
        {
            if (statements != null)
                children = statements.ToList();

        }
    }
}