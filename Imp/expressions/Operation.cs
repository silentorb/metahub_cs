using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.imperative.expressions
{
    public class Operation : Expression
    {
        public string op;

        public Operation(string op, IEnumerable<Expression> expressions)
            : base(Expression_Type.operation)
        {
            this.op = op;

#if DEBUG
            if (expressions.Any(e => e == null))
                throw new Exception("Argument cannot be null");
#endif
   
            add(expressions);
        }

        public Operation(string op, Expression first, Expression second)
            : base(Expression_Type.operation)
        {
            this.op = op;
            add(first);
            add(second);
        }

        public bool is_condition()
        {
            return op == "=="
                 || op == "!="
                 || op == ">="
                 || op == "<="
                 || op == ">"
                 || op == "<"
             ;
        }

        public override Expression clone()
        {
            return new Operation(op, children.Select(e => e.clone()));
        }

        public override bool is_empty()
        {
            return children.Count > 0;
        }
    }
}
