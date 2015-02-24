using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;

namespace metahub.imperative.expressions
{
    public class Iterator : Expression
    {
        public Symbol parameter;
        public Expression expression;
        public List<Expression> children;

        public Iterator(Symbol parameter, Expression expression, List<Expression> children)
            : base(Expression_Type.iterator)
        {
            this.parameter = parameter;
            this.expression = expression;
            this.children = children;
        }
    }
}
