using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.logic.nodes;

namespace metahub.imperative.types
{
    public class Function_Call : Expression
    {
        public Expression[] args;
        public Expression reference;

        public Function_Call(Expression_Type type, Expression reference = null, IEnumerable<Expression> args = null)
            : base(type)
        {
            this.args = args != null ? args.ToArray() : new Expression[0];
            this.reference = reference;
        }

    }

}