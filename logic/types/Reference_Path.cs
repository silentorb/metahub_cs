using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;

namespace metahub.logic.types
{
    public class Reference_Path : Node
    {
        public List<Node> children;

        public Reference_Path(IEnumerable<Node> children)
            : base(Node_Type.path)
        {
            this.children = children.ToList();
        }

        override public Signature get_signature()
        {
            if (children.Count == 0)
                throw new Exception("Cannot find signature of empty array.");

            return children[children.Count - 1].get_signature();
        }
    }
}