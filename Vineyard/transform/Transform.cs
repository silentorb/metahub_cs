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
        public Dictionary<Node, Node> map;
        public Node origin;
        public Node new_origin;

        public Transform(Node origin)
        {
            this.origin = origin;
            new_origin = origin;
        }

        public Node get_transformed(Node input)
        {
            if (map == null)
                return input;

            return map[input];
        }

        public Transform center_on(Node node)
        {
            if (origin.connections.OfType<Function_Node>().All(n => !n.is_operation))
                return this;

            clone_once();

            if (map.ContainsKey(node))
                node = map[node];

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

            return this;
        }

        public Transform change_context(Property_Node new_context)
        {
            if (new_context.aggregate(Dir.In).Count() == 1)
                return this;

            clone_once();

            if (map.ContainsKey(new_context))
                new_context = (Property_Node)map[new_context];

            var tokens = new_context.aggregate(Dir.In, null, true).ToList();
            var original_context = (Scope_Node)tokens.Last();
            tokens.RemoveAt(tokens.Count - 1);
            tokens.Reverse();

            var others = original_context.get_other_outputs(tokens.Last()).OfType<Property_Node>().ToArray();

            var trellis = original_context.trellis;

            foreach (var other in others)
            {
                var last = other;
                foreach (Property_Node token in tokens)
                {
                    if (token.property.other_trellis == null)
                        continue;

                    var new_property = token.property.other_trellis.get_reference(trellis);
                    var property_node = new Property_Node(new_property);
                    Node.insert(original_context, property_node, last);
                    last = property_node;
                    trellis = last.property.trellis;
                }
            }

            original_context.trellis = new_context.property.trellis;
            new_context.replace_other(new_context.inputs[0], original_context);

            return this;
        }

        public void clone_once()
        {
            if (map == null)
            {
                map = new Dictionary<Node, Node>();
                new_origin = clone_all(new_origin);
            }
        }

        Node clone_all(Node node, bool clone_outputs = true)
        {
            var result = node.clone();

            map.Add(node, result);

            foreach (var connection in node.inputs)
            {
                var other = map.ContainsKey(connection)
                     ? map[connection]
                     : clone_all(connection, false);

                result.connect_input(other);
            }

            if (clone_outputs)
            {
                foreach (var connection in node.outputs)
                {
                    var other = map.ContainsKey(connection)
                                    ? map[connection]
                                    : clone_all(connection);

                    result.connect_output(other);
                }
            }

            return result;
        }
    }
}
