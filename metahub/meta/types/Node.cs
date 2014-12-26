

/**
 * @author Christopher W. Johnson
 */

using System;

namespace metahub.meta.types
{
public class Node
    {
        public Node_Type type;

        protected Node(Node_Type type)
        {
            this.type = type;
        }

        virtual public metahub.logic.schema.Signature get_signature()
        {
            throw new Exception(GetType().Name + " does not implement get_signature().");
        }
    }
}