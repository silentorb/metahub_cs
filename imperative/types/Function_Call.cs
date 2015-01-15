using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.logic.types;

namespace metahub.imperative.types
{
    public class Function_Call : Expression
    {
        public string name;
        public Expression[] args;
        public bool is_platform_specific;
        public Imp imp;
        public Expression reference;

        public Function_Call(string name, Expression reference = null, IEnumerable<Expression> args = null, bool is_platform_specific = false)
            : base(Expression_Type.function_call)
        {
            this.name = name;
            this.is_platform_specific = is_platform_specific;
            this.args = args != null ? args.ToArray() : new Expression[0];
            this.reference = reference;
        }

        public Function_Call(Imp imp, IEnumerable<Expression> args = null)
            : base(Expression_Type.function_call)
        {
            this.imp = imp;
            this.args = args != null ? args.ToArray() : new Expression[0];

            name = imp.name;
            is_platform_specific = imp.is_platform_specific;
        }

        public Function_Call set_reference(Expression reference)
        {
            this.reference = reference;
            return this;
        }
    }

}