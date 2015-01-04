using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.imperative.types
{
   public class Operation : Expression
   {
       public string op;
       public Expression[] expressions;

       public Operation(string op, IEnumerable<Expression> expressions)
           : base(Expression_Type.operation)
       {
           this.op = op;
           this.expressions = expressions.ToArray();
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
    }
}
