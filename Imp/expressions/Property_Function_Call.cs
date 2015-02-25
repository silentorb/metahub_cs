using System;
using System.Collections.Generic;
using metahub.imperative.schema;



namespace metahub.imperative.expressions
{
    public enum Property_Function_Type
    {
        get,
        set,
        remove
    }

    public class Property_Function_Call : Expression
    {
        public Portal portal;
        public Property_Function_Type function_type;
        public List<Expression> args;
        private Expression reference1;

        public Property_Function_Call(Property_Function_Type function_type, Portal portal, List<Expression> args = null)
            : base(Expression_Type.property_function_call)
        {
            this.function_type = function_type;
            this.portal = portal;
            this.args = args ?? new List<Expression>();
        }

        public Expression reference
        {
            get { return reference1; }
            set
            {
                if (value != null && value.type == Expression_Type.operation)
                            throw new Exception("Cannot call function on operation.");

                reference1 = value;
            }
        }

        //public Property_Function_Call set_reference(Expression reference)
        //{
        //    this.reference = reference;
        //    if (reference.type == Expression_Type.operation)
        //        throw new Exception("Cannot call function on operation.");

        //    return this;
        //}

        public override Expression clone()
        {
            return new Property_Function_Call(function_type, portal, args)
                {
                    reference = reference
                };
        }
    }

}