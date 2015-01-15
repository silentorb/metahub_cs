using System.Collections.Generic;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.types;

namespace metahub.imperative.types
{
    public enum Property_Function_Type
    {
        get,
        set,
        remove
    }

    public class Property_Function_Call : Expression
    {
        public Tie tie;
        public Portal portal;
        public Property_Function_Type function_type;
        public List<Expression> args;
        public Expression reference;

        public Property_Function_Call(Property_Function_Type function_type, Tie tie, List<Expression> args = null)
            : base(Expression_Type.property_function_call)
        {
            this.function_type = function_type;
            this.tie = tie;
            this.args = args ?? new List<Expression>();
        }

        public Property_Function_Call(Property_Function_Type function_type, Portal portal, List<Expression> args = null)
            : base(Expression_Type.property_function_call)
        {
            this.function_type = function_type;
            this.portal = portal;
            this.args = args ?? new List<Expression>();
        }

        public Property_Function_Call set_reference(Expression reference)
        {
            this.reference = reference;
            return this;
        }
    }

}