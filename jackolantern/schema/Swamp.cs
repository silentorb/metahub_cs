using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.jackolantern.schema
{
    public class Swamp
    {
        public enum Dir
        {
            In,
            Out
        }
        public JackOLantern jack;
        public Node end;

        public Swamp(JackOLantern jack, Node end)
        {
            this.jack = jack;
            this.end = end;
        }

        //public Expression translate(Node expression, Node previous, Dir dir, Scope context = null)
        //{
        //    var result = _translate(expression, previous, dir, context);
        //    if (expression.inputs.Count > 0)
        //    {
        //        if (result.child != null)
        //            throw new Exception("Looks like a ");

        //        var input = expression.inputs[0];
        //        if (input.type == Node_Type.function_call)
        //        {
        //            var function_call = (metahub.logic.nodes.Function_Call)input;
        //            return new Platform_Function(function_call.name, result, null)
        //            {
        //                profession = new Profession(function_call.signature, jack.overlord)
        //            };
        //        }

        //        result.child = translate(expression.inputs[0], previous, dir);
        //    }

        //    return result;
        //}
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
        IEnumerable<Expression> translate_many(IEnumerable<Node> nodes, Node previous, Dir dir, Summoner.Context context)
        {
            return nodes.Select(n => translate(n, previous, dir, context));
        }

        Expression translate_node_scope(Node node, Node previous, Dir dir, Summoner.Context context)
        {
            if (dir == Dir.Out)
                throw new Exception("Infinite Loop!");

            foreach (var output in node.outputs)
            {
                if (output != previous)
                {
                    return translate(output, node, Dir.Out, context);
                }
            }

            throw new Exception("Couldn't find other node.");
        }

        public Expression translate(Node node, Node previous, Dir dir, Summoner.Context context, int step = 0)
        {
            if (node.type == Node_Type.scope_node && step == 0)
            {
                return translate(node.outputs.First(o => o != previous), node, Dir.Out, context, 1);
            }

            var expression = get_expression(node, previous, dir, context);
            if (expression == null)
                return null;

            Node next = get_next(node, previous, ref dir);
            if (next == null)
                return expression;

            var child = translate(next, node, dir, context, step + 1);
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

        public Expression translate2(Node node, Node previous, Dir dir, Summoner.Context context, int step = 0)
        {
            var next = get_next(node, previous, ref dir, step);
            if (next == null)
            {
                if (step == 0)
                    throw new Exception("Empty node path.");

                return null;
            }
            var expression = get_expression(next, node, dir, context);
            if (expression == null)
                throw new Exception();

            var child = translate2(next, node, dir, context, step + 1);
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
                result = dir == Dir.In
                    ? node.inputs[0]
                    : node.outputs[0];
            }

            if (result == end)
                return null;


            return result;
        }

        Expression get_expression(Node node, Node previous, Dir dir, Summoner.Context context)
        {
            switch (node.type)
            {
                case Node_Type.property:
                    {
                        var property_node = (Property_Reference)node;
                        var tie = dir == Dir.Out
                                      ? property_node.tie
                                      : ((Property_Reference)previous).tie.other_tie;
                        if (tie == null)
                            return null;

                        return new Portal_Expression(jack.overlord.get_portal(tie));
                    }

                case Node_Type.variable:
                    var variable = (metahub.logic.nodes.Variable)node;
                    return context.scope.resolve(variable.name);

                case Node_Type.scope_node:
                    {
                        var property_node = (Property_Reference)previous;
                        var tie = property_node.tie.other_tie;
                        return new Portal_Expression(jack.overlord.get_portal(tie));
                    }

                case Node_Type.function_call:
                    {
                        var function_call = (Function_Call2)node;
                        var args = function_call.inputs.Select(i => get_expression(i, null, Dir.In, context));
                        return new Platform_Function(function_call.name, null, args);
                    }
            }

            throw new Exception("Not yet supported: " + node.type);
        }
    }
}
