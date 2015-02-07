using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic;
using metahub.logic.nodes;

namespace metahub.jackolantern.tools
{
    public class Transform
    {
        public Dictionary<Node, Node> map = new Dictionary<Node, Node>();
        public Node origin;
        public Node new_origin;

        private Transform(Node origin)
        {
            this.origin = origin;
            new_origin = origin;
        }

        public Node get_out(Node input)
        {
            if (map.Count == 0)
                return input;

            return map[input];
        }

        public static Transform center_on(Node origin)
        {
            var transform = new Transform(origin);
            //if (new[] { origin }.Concat(origin.outputs).OfType<Function_Call2>().All(n => !n.is_operation))
            if (origin.outputs.OfType<Function_Call2>().All(n => !n.is_operation))
                return transform;

            var node = clone_all(origin, transform.map);
            if (node.outputs.Count > 1)
                throw new Exception("Not yet supported.");

            var operation = (Function_Call2)node.outputs[0];
            operation.name = Logician.inverse_operators[operation.name];

            if (operation.outputs.Count > 1)
                throw new Exception("Not yet supported.");

            // Prepare for 
            var join = (Function_Call2)operation.outputs[0];
            join.name = Logician.inverse_operators[join.name];
            var other_side = join.get_other_input(operation);

            // Perform the transformation, similar to rotating a rubix cube.
            operation.replace_connection(node, other_side);
            join.replace_connection(operation, node);
            join.replace_connection(other_side, operation);

            transform.new_origin = node;
            return transform;
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
