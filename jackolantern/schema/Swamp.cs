using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
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

        //public Expression translate(Node expression, Node previous, Dir dir, Scope scope = null)
        //{
        //    var result = _translate(expression, previous, dir, scope);
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

        public Expression translate_old(Node node, Node previous, Dir dir, Scope scope = null)
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
                //    return convert_path(((metahub.logic.nodes.Reference_Path)expression).children, scope);

                //case Node_Type.array:
                //    return new Create_Array(translate_many(((metahub.logic.nodes.Array_Expression)expression).children, scope));

                //case Node_Type.block:
                //    return new Create_Array(translate_many(((metahub.logic.nodes.Block)expression).children, scope));

                //case Node_Type.lambda:
                //    return null;

                case Node_Type.scope_node:
                    return translate_node_scope(node, previous, dir, scope);

                case Node_Type.variable:
                case Node_Type.property:
                    return translate(node, previous, dir, scope);

                case Node_Type.operation:
                    var operation = (metahub.logic.nodes.Operation_Node)node;
                    return new Operation(operation.op, translate_many(operation.children, previous, dir, scope));

                default:
                    throw new Exception("Cannot convert node " + node.type + ".");
            }
        }

        IEnumerable<Expression> translate_many(IEnumerable<Node> nodes, Node previous, Dir dir, Scope scope)
        {
            return nodes.Select(n => translate(n, previous, dir, scope));
        }

        Expression translate_node_scope(Node node, Node previous, Dir dir, Scope scope)
        {
            if (dir == Dir.Out)
                throw new Exception("Infinite Loop!");

            foreach (var output in node.outputs)
            {
                if (output != previous)
                {
                    return translate(output, node, Dir.Out, scope);
                }
            }

            throw new Exception("Couldn't find other node.");
        }

        public Expression translate(Node node, Node previous, Dir dir, Scope scope)
        {
            var expression = get_expression(node, previous, dir, scope);
            if (expression == null)
                return null;

            Node next = null;
            if (node.type == Node_Type.scope_node)
            {
                if (dir == Dir.Out)
                    throw new Exception("Infinite Loop!");

                next = node.outputs.First(o => o != previous);
                dir = Dir.Out;
            }
            else
            {
                next = get_next(node, previous, dir);
            }
            if (next == null)
                return expression;

            expression.child = translate(next, node, dir, scope);
            return expression;
        }

        Node get_next(Node node, Node previous, Dir dir)
        {
            
            var result = dir == Dir.In
                ? node.inputs[0]
                : node.outputs[0];

            return result == end
                ? null
                : result;
        }

        Expression get_expression(Node node, Node previous, Dir dir, Scope scope)
        {
            switch (node.type)
            {
                case Node_Type.property:
                    {
                        var property_node = (Property_Reference) node;
                        var tie = dir == Dir.In
                                      ? property_node.tie
                                      : property_node.tie.other_tie;
                        if (tie == null)
                            return null;

                        return new Portal_Expression(jack.overlord.get_portal(tie));
                    }

                case Node_Type.variable:
                    var variable = (metahub.logic.nodes.Variable)node;
                    return scope.resolve(variable.name);

                case Node_Type.scope_node:
                    {
                        var property_node = (Property_Reference)previous;
                        var tie = property_node.tie.other_tie;
                        return new Portal_Expression(jack.overlord.get_portal(tie));
                    }
            }

            throw new Exception("Not yet supported: " + node.type);
        }

        Expression convert_path(IList<metahub.logic.nodes.Node> path, Scope scope = null)
        {
            List<Expression> result = new List<Expression>();
            Rail rail = null;
            Dungeon dungeon = null;

            if (path.First().type == Node_Type.property)
            {
                rail = ((metahub.logic.nodes.Property_Reference)path.First()).tie.get_abstract_rail();
            }
            //else
            //{
            //    rail = ((metahub.logic.types.Array_Expression)path[0])
            //}
            foreach (var token in path)
            {
                switch (token.type)
                {
                    case Node_Type.property:
                        var property_token = (metahub.logic.nodes.Property_Reference)token;
                        if (dungeon != null)
                        {
                            var portal = dungeon.all_portals[property_token.tie.name];
                            if (portal == null)
                                throw new Exception("Portal is null: " + property_token.tie.fullname());

                            result.Add(new Portal_Expression(portal));
                            dungeon = portal.other_dungeon;
                        }
                        else
                        {
                            var tie = rail.all_ties[property_token.tie.name];
                            if (tie == null)
                                throw new Exception("tie is null: " + property_token.tie.fullname());

                            result.Add(new Portal_Expression(jack.overlord.get_portal(tie)));
                            rail = tie.other_rail;
                        }
                        break;

                    case Node_Type.array:
                        //result.Add(translate(token, scope));
                        break;

                    case Node_Type.variable:
                        var variable = (metahub.logic.nodes.Variable)token;
                        var variable_token = scope.resolve(variable.name);
                        result.Add(variable_token);
                        var profession = variable_token.get_profession();
                        if (profession == null)
                        {
                            rail = variable_token.get_signature().rail;
                        }
                        else if (profession.dungeon != null)
                        {
                            if (profession.dungeon.rail != null)
                                rail = profession.dungeon.rail;

                            dungeon = profession.dungeon;
                        }
                        else if (profession.type == Kind.list || profession.type == Kind.reference)
                        {
                            throw new Exception("Invalid profession.");
                        }
                        break;

                    case Node_Type.function_call:
                    case Node_Type.function_scope:
                        var function_token = (metahub.logic.nodes.Function_Call)token;
                        result.Add(new Platform_Function(function_token.name, null,
                            new List<Expression>()));
                        break;

                    default:
                        throw new Exception("Invalid path token: " + token.type);
                }
            }
            return package_path(result);
        }

        Expression package_path(IEnumerable<Expression> path)
        {
            if (path.Count() == 1)
                return path.First();

            return new Path(path);
        }
    }
}
