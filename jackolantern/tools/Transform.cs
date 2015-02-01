using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic;
using metahub.logic.nodes;

namespace metahub.jackolantern.tools
{
   public static class Transform
    {
       public static Node center_on(Node original)
       {
           if (original.outputs.All(n => n.type != Node_Type.function_call))
               return original;

           var node = clone_all(original, new Dictionary<Node, Node>());
           if (node.outputs.Count > 1)
               throw new Exception("Not yet supported.");

           var operation = (Function_Call2) node.outputs[0];
           operation.name = Logician.inverse_operators[operation.name];

           if (operation.outputs.Count > 1)
               throw new Exception("Not yet supported.");

           // Prepare for 
           var join = (Function_Call2) operation.outputs[0];
           join.name = Logician.inverse_operators[join.name];
           var other_side = join.get_other_input(operation);

           // Perform the transformation, similar to rotating a rubix cube.
           operation.replace(node, other_side);
           join.replace(operation, node);
           join.replace(other_side, operation);

           return node;
       }

       public static Node clone_all(Node node, Dictionary<Node, Node> map)
       {   
           var result = node.clone();
           map.Add(node, result);

           foreach (var connection in node.inputs)
           {
               var other = map.ContainsKey(connection)
                    ? map[connection]
                    : clone_all(connection, map);

               result.connect_input(other);
           }

           foreach (var connection in node.outputs)
           {
               var other = map.ContainsKey(connection)
                    ? map[connection]
                    : clone_all(connection, map);

               result.connect_output(other);
           }

           return result;
       }
    }
}
