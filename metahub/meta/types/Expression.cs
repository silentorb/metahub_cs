

/**
 * @author Christopher W. Johnson
 */

using System;

namespace metahub.meta.types
{
public class Expression
    {
        public Expression_Type type;

        protected Expression(Expression_Type type)
        {
            this.type = type;
        }

        public metahub.logic.schema.Signature get_signature()
        {
            throw new Exception(GetType().Name + " does not implement get_signature().");
        }
    }
}