using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.imperative.types
{
    public class Statements : Expression
    {
        public List<Expression> children;

        public Statements(List<Expression> children = null)
            : base(Expression_Type.statements)
        {
            this.children = children ?? new List<Expression>();
        }

        public override Expression clone()
        {
            return new Statements(children.Select(c => c.clone()).ToList());
        }
    }
}
