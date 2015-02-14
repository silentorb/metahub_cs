using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.jackolantern.tools;
using metahub.logic.schema;

namespace metahub.logic.nodes
{
   public class Scope_Node : Node
   {
       public Rail rail;

       public Scope_Node(Rail rail)
           :base(Node_Type.scope_node)
       {
           if (rail == null)
               throw new Exception("Scope_Node.rail cannot be null.");

           this.rail = rail;
       }

       public override string debug_string
       {
           get { return "Scope " + rail.name; }
       }

       public override Node clone()
       {
           return new Scope_Node(rail);
       }
    }
}
