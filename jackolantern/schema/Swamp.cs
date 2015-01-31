using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.code;
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

        static Dir reverse(Dir dir)
        {
            return dir == Dir.Out
                ? Dir.In
                : Dir.Out;
        }

        JackOLantern jack;
        Node end;
        Summoner.Context context;

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
            return nodes.Select(n => translate(n, previous, dir));
        }

        public Expression translate(Node node, Node previous, Dir dir, int step = 0)
        {
            if (node.type == Node_Type.scope_node && step == 0)
            {
                return translate(node.outputs.First(o => o != previous), node, Dir.Out, 1);
            }

            var expression = get_expression(node, previous, dir);
            if (expression == null)
                return null;

            Node next = get_next(node, previous, ref dir);
            if (next == null)
                return expression;

            var child = translate(next, node, dir, step + 1);
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

        public Expression translate2(Node node, Node previous, Dir dir, int step = 0)
        {
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

            var child = translate2(next, node, dir, step + 1);
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

        Expression get_expression(Node node, Node previous, Dir dir)
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
                        var args = function_call.inputs.Select(i => get_expression(i, null, Dir.In));
                        return new Platform_Function(function_call.name, null, args);
                    }

                case Node_Type.literal:
                    var literal = (Literal_Value)node;
                    return new Literal(literal.value, new Profession(literal.kind));

                case Node_Type.operation:
                    var operation = (Operation_Node)node;
                    var op = dir == Dir.Out
                        ? operation.op
                        : Reference.inverse_operators[operation.op];

                    return new Operation(op, operation.inputs.Select(i => translate2(i, operation, Dir.In)));
            }

            throw new Exception("Not yet supported: " + node.type);
        }
    }
}
