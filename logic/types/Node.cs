using System;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.types
{
    public class Node
    {
        public Node_Type type;

        protected Node(Node_Type type)
        {
            this.type = type;
        }

        virtual public Signature get_signature()
        {
            //throw new Exception(GetType().Name + " does not implement get_signature().");
            return new Signature(Kind.unknown);
        }
    }
}