using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.expressions;

namespace metahub.jackolantern.code
{
   public class Ration
   {
       public Dwarf dwarf;
       public Expression target;
       public List<Expression> expressions = new List<Expression>();

       public Ration(Dwarf dwarf, Expression target)
       {
           this.dwarf = dwarf;
           this.target = target;
       }

       public static List<Expression> get_portal_path(Expression expression)
        {
            var result = new List<Expression>();
            var current = expression;
            do
            {
                result.Add(current);
                current = current.next;
            } while (current != null && current.type == Expression_Type.portal);

            return result;
        }

       public Operation create_null_check()
       {
           return new Operation("!=", target, new Null_Value());
       }
   }
}
