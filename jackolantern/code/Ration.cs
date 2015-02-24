using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.expressions;

namespace metahub.jackolantern.code
{
   public class Ration
   {
       public Pickaxe pickaxe;
       public Expression target;
       public List<Expression> expressions = new List<Expression>();

       public Ration(Pickaxe pickaxe, Expression target)
       {
           this.pickaxe = pickaxe;
           this.target = target;
       }
   }
}
