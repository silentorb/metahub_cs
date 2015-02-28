using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern.code;
using metahub.jackolantern.tools;
using metahub.logic;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.jackolantern.schema
{
    public class Swamp
    {
        static Dir reverse(Dir dir)
        {
            return dir == Dir.Out
                ? Dir.In
                : Dir.Out;
        }

        JackOLantern jack;
        Node end;
        public Summoner.Context context;
        public Node previous_node;
        public Node last_node;

        public Swamp(JackOLantern jack, Node end, Summoner.Context context)
        {
            this.jack = jack;
            this.end = end;
            this.context = context;
        }

        /*
        public Expression translate_old(Node node, Node previous, Dir dir, Scope context = null)
        {
            switch (node.type)
            {
                case Node_Type.literal:
                    return new Literal(((metahub.logic.nodes.Literal_Value)node).value, new Profession(Kind.unknown));

                case Node_Type.function_call:
                    var function_call = (metahub.logic.nodes.Function_Call)node;
                    return new Platform_Function(function_call.name, null, null)
                    {
                        profession = new Profession(function_call.signature, jack.overlord)
                    };

                //case Node_Type.path:
                //    return convert_path(((metahub.logic.nodes.Reference_Path)expression).children, context);

                //case Node_Type.array:
                //    return new Create_Array(translate_many(((metahub.logic.nodes.Array_Expression)expression).children, context));

                //case Node_Type.block:
                //    return new Create_Array(translate_many(((metahub.logic.nodes.Block)expression).children, context));

                //case Node_Type.lambda:
                //    return null;

                case Node_Type.scope_node:
                    return translate_node_scope(node, previous, dir, context);

                case Node_Type.variable:
                case Node_Type.property:
                    return translate(node, previous, dir, context);

                case Node_Type.operation:
                    var operation = (metahub.logic.nodes.Operation_Node)node;
                    return new Operation(operation.op, translate_many(operation.children, previous, dir, context));

                default:
                    throw new Exception("Cannot convert node " + node.type + ".");
            }
        }
        */
        //IEnumerable<Expression> translate_many(IEnumerable<Node> nodes, Node previous, Dir dir)
        //{
        //    return nodes.Select(n => translate_inclusive(n, previous, dir));
        //}

        //public Expression translate_inclusive(Node node, Node previous, Dir dir, int step = 0)
        //{
        //    if (node.type == Node_Type.scope_node && step == 0)
        //    {
        //        return translate_inclusive(node.outputs.First(o => o != previous), node, Dir.Out, 1);
        //    }

        //    var expression = get_expression(node, previous, dir);
        //    if (expression == null)
        //        return null;

        //    Node next = get_next(node, previous, ref dir);
        //    if (next == null)
        //        return expression;

        //    var child = translate_inclusive(next, node, dir, step + 1);
        //    if (child != null && child.type == Expression_Type.platform_function)
        //    {
        //        child.child = expression;
        //        return child;
        //    }
        //    else
        //    {
        //        expression.child = child;
        //        return expression;
        //    }
        //}

        public Expression translate_exclusive(Node node, Node previous, Dir dir, int step = 0)
        {
            var chain = get_exclusive_chain(node, previous, dir);
            if (chain.Count == 0)
            {
                get_exclusive_chain(node, previous, dir);
                throw new Exception("Chain has no nodes.");
            }

            var result = render_chain(chain);
            //if (result == null)
            //    throw new Exception("Could not render node chain.");

            return result;
        }

        public Expression translate_inclusive(Node node, Node previous, Dir dir, int step = 0)
        {
            var chain = get_inclusive_chain(node, previous, dir);
            if (chain.Count == 0)
            {
                get_exclusive_chain(node, previous, dir);
                throw new Exception("Chain has no nodes.");
            }

            var result = render_chain(chain);
            if (result == null)
                throw new Exception("Could not render node chain.");

            return result;
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
                if (result == end)
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
                    if (property_node.tie.trellis.is_value)
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

        Expression get_expression(Node node, Node previous, Dir dir)
        {
            switch (node.type)
            {
                case Node_Type.property:
                    {
                        var property_node = (Property_Node)node;
                        var tie = dir == Dir.Out
                                      ? property_node.tie
                            //: ((Property_Node)previous).tie.other_tie;
                                      : ((Property_Node)previous).tie.trellis.get_reference(property_node.tie.other_trellis)
                                      ?? ((Property_Node)previous).tie.other_property;

                        if (tie == null)
                            throw new Exception("Not supported.");
                        //return null;

                        return new Portal_Expression(jack.get_portal(tie));
                    }

                case Node_Type.variable:
                    var variable = (metahub.logic.nodes.Variable_Node)node;
                    return context.scope.resolve(variable.name, context);

                case Node_Type.scope_node:
                    {
                        var property_node = (Property_Node)previous;
                        var scope_node = (Scope_Node)node;

                        Property tie;
                        if (scope_node.rail == property_node.tie.trellis)
                        {
                            if (property_node.tie.other_trellis.is_value)
                                return null;

                            tie = property_node.tie.other_property;
                        }
                        else
                        {
                            tie = property_node.tie.trellis.get_reference(scope_node.rail);
                        }

                        if (tie == null)
                            throw new Exception("Property cannot be null.");

                        return new Portal_Expression(jack.get_portal(tie));
                    }

                //case Node_Type.function_call:
                //    {
                //        var function_call = (Function_Call2)node;
                //        var args = function_call.inputs.Select(i => get_expression(i, null, Dir.In));
                //        return new Platform_Function(function_call.name, null, args);
                //    }

                case Node_Type.literal:
                    var literal = (Literal_Value)node;
                    return new Literal(literal.value, new Profession(literal.kind));

                case Node_Type.function_call:
                    var operation = (Function_Node)node;

                    if (operation.is_operation)
                    {
                        var op = dir == Dir.Out
                                     ? operation.name
                                     : Logician.inverse_operators[operation.name];

                        var inputs = operation.inputs
                                              .Where(i => i.type != Node_Type.lambda)
                                              .Select(i => translate_backwards(i, operation));

                        return new Operation(op, inputs);
                    }
                    else
                    {
                        return new Platform_Function(operation.name,
                            translate_backwards(operation.inputs[0], operation),
                            operation.inputs.Skip(1)
                            .Select(i => translate_backwards(i, operation)
                            ));
                    }
            }

            throw new Exception("Not yet supported: " + node.type);
        }

        public Expression translate_backwards(Node node, Node previous, int step = 0)
        {
            if (step > 10)
                throw new Exception("Not supported.");

            var dir = Dir.In;

            var expression = get_expression(node, previous, Dir.Out);
            if (expression == null)
                throw new Exception();

            if (node.type != Node_Type.property)
                return expression;

            var next = get_next(node, previous, ref dir, step, true);

            if (next == null || next == end)// || next.type == Node_Type.scope_node)
                return expression;

            if (next.type == Node_Type.property
                && ((Property_Node)next).tie.other_trellis == jack.get_rail(context.dungeon))
            {
                return expression;
            }

            var child = translate_backwards(next, node, step + 1);
            if (child != null && child.type == Expression_Type.platform_function)
            {
                expression.next = child;
                return expression;
            }
            else if (child != null)
            {
                get_last_child(child).next = expression;
                return child;
            }

            if (expression == null)
                throw new Exception("expression cannot be null.");

            return expression;
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

        public Expression render_chain(List<Node_Link> links)
        {
            Expression result = null;
            Expression last = null;

            foreach (var link in links)
            {
                var expression = get_expression(link.node, link.previous, link.dir);
                if (expression == null)
                    continue;

                if (result == null)
                {
                    result = expression;
                }
                else
                {
                    last.next = expression;
                }

                last = expression;
            }

            return result;
        }

        static Expression get_last_child(Expression expression)
        {
            while (expression.next != null)
            {
                expression = expression.next;
            }
            return expression;
        }

        public Expression[] get_expression_pair(Node node)
        {
            //var original_target = get_exclusive_chain(node, null, Dir.In);
            var transform = Transform.center_on(node);
            var new_target = transform.get_transformed(node);
            var lvalue = transform.get_transformed(node);
            var rvalue = transform.get_transformed(end).get_other_input(new_target);
            var parent = lvalue.inputs[0];
            var lexpression = translate_backwards(lvalue, null);
            var rexpression = translate_backwards(rvalue, null);
            //var has_transforms = end.aggregate(Dir.In).OfType<Function_Node>().Any(n => n.is_operation);

            //return has_transforms
            //    ? new[] { lexpression, rexpression }
            //    : new[] { rexpression, lexpression };

            return new[] { lexpression, rexpression };
        }

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
