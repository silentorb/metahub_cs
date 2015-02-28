using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes
{
   public class Scope_Node : Node
   {
       public Trellis rail;

       public Scope_Node(Trellis rail)
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
