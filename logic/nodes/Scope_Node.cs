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
           this.rail = rail;
       }

       public override Node clone(Transform transform)
       {
           return new Scope_Node(rail);
       }
    }
}
