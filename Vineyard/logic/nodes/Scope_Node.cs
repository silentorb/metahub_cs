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
       public Trellis trellis;

       public Scope_Node(Trellis trellis)
           :base(Node_Type.scope_node)
       {
           if (trellis == null)
               throw new Exception("Scope_Node.rail cannot be null.");

           this.trellis = trellis;
       }

       public override string debug_string
       {
           get { return "Scope " + trellis.name; }
       }

       public override Node clone()
       {
           return new Scope_Node(trellis);
       }
    }
}
