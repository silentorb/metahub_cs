using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic;
using metahub.logic.nodes;

namespace vineyard.transform
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

        public Node get_transformed(Node input)
        {
            if (map.Count == 0)
                return input;

            return map[input];
        }

        public static Transform center_on(Node origin)
        {
            var transform = new Transform(origin);
            if (origin.outputs.OfType<Function_Node>().All(n => !n.is_operation))
                return transform;

            var node = clone_all(origin, transform.map);
            if (node.outputs.Count > 1)
                throw new Exception("Not yet supported.");

            var operation = (Function_Node)node.outputs[0];
            operation.name = operation.get_inverse();

            if (operation.outputs.Count > 1)
                throw new Exception("Not yet supported.");

            // Prepare for 
            var join = (Function_Node)operation.outputs[0];
            join.name = join.get_inverse();
            var other_side = join.get_other_input(operation);
            join.inputs.Reverse();

            // Perform the transformation, similar to rotating a rubix cube.
            operation.replace_other(node, other_side);
            join.replace_other(operation, node);
            join.replace_other(other_side, operation);

            transform.new_origin = node;
            return transform;
        }

        public static Transform change_context(Node root, Property_Node new_context)
        {
            var transform = new Transform(root);
            if (new_context.aggregate(Dir.In).Count() == 1)
                return transform;

            transform.new_origin = clone_all(root, transform.map);
            new_context = (Property_Node)transform.get_transformed(new_context);
            var tokens = new_context.aggregate(Dir.In).ToList();
            var original_context = (Scope_Node)tokens.Last();
            tokens.RemoveAt(tokens.Count - 1);

            var others = original_context.get_other_outputs(tokens.Last()).ToArray();

            var trellis = original_context.trellis;
             
            foreach (var other in others)
            {
                var last = other;
                foreach (Property_Node token in tokens)
                {
                    var new_property = token.property.trellis.get_reference(trellis);
                    var property_node = new Property_Node(new_property);
                    Node.insert(original_context, property_node, last);
                    last = property_node;
                }
            }

            original_context.trellis = new_context.property.trellis;
            new_context.replace_other(new_context.inputs[0], original_context);

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
