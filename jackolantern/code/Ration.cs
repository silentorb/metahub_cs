using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.expressions;

namespace metahub.jackolantern.code
{
   public class Ration
   {
       public Dwarf pickaxe;
       public Expression target;
       public List<Expression> expressions = new List<Expression>();

       public Ration(Dwarf pickaxe, Expression target)
       {
           this.pickaxe = pickaxe;
           this.target = target;
       }

       public static List<Expression> get_portal_path(Expression expression)
        {
            var result = new List<Expression>();
            var current = expression;
            do
            {
                result.Add(current);
                current = current.child;
            } while (current != null && current.type == Expression_Type.portal);

            return result;
        }

   }
}
