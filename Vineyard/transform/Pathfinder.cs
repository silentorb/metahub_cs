using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic.nodes;
using metahub.schema;

namespace vineyard.transform
{
    public class Pathfinder
    {
        static Dir reverse(Dir dir)
        {
            return dir == Dir.Out
                ? Dir.In
                : Dir.Out;
        }

        Trellis context;
        public Node previous_node;
        public Node last_node;

        public Pathfinder(Trellis context)
        {
            this.context = context;
        }

        Node get_next(Node node, Node previous, ref Dir dir, int step = 0, bool is_backwards = false)
        {
            Node result;
            if (node.type == Node_Type.scope_node)
            {
                if (step > 0 && dir == Dir.Out)
                    throw new Exception("Infinite Loop!");

                dir = Dir.Out;
                result = node.outputs.FirstOrDefault(o => o != previous);
                if (result == null)
                    return null;
            }
            else if (node.type == Node_Type.bounce)
            {
                if (is_backwards)
                    return null;

                if (previous == null)
                    throw new Exception("Bounce must have a previous node.");

                dir = Dir.Out;
                result = node.outputs.First(o => o != previous).outputs.First();
            }
            else
            {
                if (dir == Dir.In)
                {
                    if (node.inputs.Count == 0)
                        return null;

                    result = node.inputs[0];
                }
                else
                {
                    if (node.outputs.Count == 0)
                        return null;

                    result = node.outputs[0];
                }
            }

            while (true)
            {
                if (result.type == Node_Type.property && ((Property_Node)result).property.trellis == context)
                    return null;

                if (result.type == Node_Type.bounce)
                {
                    if (is_backwards)
                        return null;

                    dir = Dir.Out;
                    result = result.outputs.First(o => o != node).outputs.First();
                }
                else if (result.type == Node_Type.property && dir == Dir.In)
                {
                    var property_node = (Property_Node)result;
                    if (property_node.property.trellis.is_value)
                        result = property_node.inputs[0];
                    else
                        break;
                }
                else
                {
                    break;
                }
            }

            if (result.type == Node_Type.function_call)
            {
                if (((Function_Node)result).name == "=")
                {
                    previous_node = node;
                    last_node = result;
                    return null;
                }

                return null;
            }

            return result;
        }

        public List<Node_Link> get_exclusive_chain(Node node, Node previous, Dir dir)
        {
            int step = -1;
            var result = new List<Node_Link>();
            do
            {
                ++step;
                Node next = get_next(node, previous, ref dir, step);
                if (next == null || (next.type == Node_Type.function_call && ((Function_Node)next).is_operation))
                    break;

                result.Add(new Node_Link(next, dir, node));
                previous = node;
                node = next;
            } while (true);

            return result;
        }

        public List<Node_Link> get_inclusive_chain(Node node, Node previous, Dir dir)
        {
            int step = -1;
            var result = new List<Node_Link>();
            do
            {
                ++step;
                result.Add(new Node_Link(node, dir, previous));
                Node next = get_next(node, previous, ref dir, step);
                if (next == null || (next.type == Node_Type.function_call && ((Function_Node)next).is_operation))
                    break;

                previous = node;
                node = next;
            } while (true);

            return result;
        }
        
//        public List<Node_Link>[] get_pair(Node node, Node context_node)
//        {
//            var transform = Transform.center_on(node);
//            var new_target = transform.get_transformed(node);
//            var lvalue = transform.get_transformed(node);
//            var rvalue = transform.get_transformed(context_node).get_other_input(new_target);
//
//            var lexpression = get_inclusive_chain(lvalue, null, Dir.In);
//            var rexpression = get_inclusive_chain(rvalue, null, Dir.In);
//      
//            return new[] { lexpression, rexpression };
//        }
        
        public static Node[] get_inputs_in_relation_to(Node pumpkin, Node primary)
        {
            if (pumpkin.inputs.Count != 2)
                throw new Exception("get_inputs_in_relation_to requires an endpoint with 2 nodes.");

            return pumpkin.inputs[0].aggregate(Dir.In).Contains(primary)
                ? pumpkin.inputs.ToArray()
                : new[] { pumpkin.inputs[1], pumpkin.inputs[0] };
        }
        
    }
}
