using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.code;
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
        Summoner.Context context;
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
        IEnumerable<Expression> translate_many(IEnumerable<Node> nodes, Node previous, Dir dir)
        {
            return nodes.Select(n => translate_inclusive(n, previous, dir));
        }

        public Expression translate_inclusive(Node node, Node previous, Dir dir, int step = 0)
        {
            if (node.type == Node_Type.scope_node && step == 0)
            {
                return translate_inclusive(node.outputs.First(o => o != previous), node, Dir.Out, 1);
            }

            var expression = get_expression(node, previous, dir);
            if (expression == null)
                return null;

            Node next = get_next(node, previous, ref dir);
            if (next == null)
                return expression;

            var child = translate_inclusive(next, node, dir, step + 1);
            if (child != null && child.type == Expression_Type.platform_function)
            {
                child.child = expression;
                return child;
            }
            else
            {
                expression.child = child;
                return expression;
            }
        }

        public Expression translate_exclusive(Node node, Node previous, Dir dir, int step = 0)
        {
            previous_node = previous;
            last_node = node;
            var next = get_next(node, previous, ref dir, step);
            if (next == null)
            {
                if (step == 0)
                    throw new Exception("Empty node path.");

                return null;
            }
            var expression = get_expression(next, node, dir);
            if (expression == null)
                throw new Exception();

            var child = translate_exclusive(next, node, dir, step + 1);
            if (child != null && child.type == Expression_Type.platform_function)
            {
                child.child = expression;
                return child;
            }
            else
            {
                expression.child = child;
                return expression;
            }
        }

        Node get_next(Node node, Node previous, ref Dir dir, int step = 0)
        {
            Node result;
            if (node.type == Node_Type.scope_node)
            {
                if (step > 0 && dir == Dir.Out)
                    throw new Exception("Infinite Loop!");

                dir = Dir.Out;
                result = node.outputs.First(o => o != previous);
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

            if (result == end)
                return null;

            if (result.type == Node_Type.function_call)
            {
                if (((Function_Node)result).name == "=")
                {
                    previous_node = node;
                    last_node = result;
                    return null;
                }
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
                                      : ((Property_Node)previous).tie.other_tie;
                        if (tie == null)
                            throw new Exception("Not supported.");
                        //return null;

                        return new Portal_Expression(jack.overlord.get_portal(tie));
                    }

                case Node_Type.variable:
                    var variable = (metahub.logic.nodes.Variable_Node)node;
                    return context.scope.resolve(variable.name);

                case Node_Type.scope_node:
                    {
                        var property_node = (Property_Node)previous;
                        var tie = property_node.tie.other_tie;
                        return new Portal_Expression(jack.overlord.get_portal(tie));
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
                    var op = dir == Dir.Out
                        ? operation.name
                        : Logician.inverse_operators[operation.name];

                    return new Operation(op, operation.inputs.Select(i => translate_backwards(i, operation)));
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

            var next = get_next(node, previous, ref dir, step);

            if (next.type == Node_Type.scope_node)
                return expression;

            if (next.type == Node_Type.property
                && ((Property_Node)next).tie.other_rail == context.dungeon.rail)
            {
                return expression;
            }

            var child = translate_backwards(next, node, step + 1);
            if (child != null && child.type == Expression_Type.platform_function)
            {
                expression.child = child;
                return expression;
            }
            else if (child != null)
            {
                get_last_child(child).child = expression;
                return child;
            }

            if (expression == null)
                throw new Exception("expression cannot be null.");

            return expression;
        }

        public List<Node> get_exclusive_chain(Node node, Dir dir)
        {
            int step = -1;
            List<Node> result = new List<Node>();
            Node previous = null;
            do
            {
                ++step;
                Node next = get_next(node, previous, ref dir, step);
                if (next == null || (next.type == Node_Type.function_call && ((Function_Node)next).is_operation))
                    break;

                result.Add(next);
                previous = node;
                node = next;
            } while (true);

            return result;
        }

        static Expression get_last_child(Expression expression)
        {
            while (expression.child != null)
            {
                expression = expression.child;
            }
            return expression;
        }

    }
}
