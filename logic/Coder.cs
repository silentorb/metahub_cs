using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;
using metahub.logic.types;
using metahub.parser;
using metahub.parser.types;
using metahub.schema;


namespace metahub.logic
{
    class Conditions_Source
    {
        public string type;
        public List<object> conditions;
        public string mode;
    }

    public class Coder
    {
        Railway railway;
        Logician logician;

        public Coder(Railway railway, Logician logician)
        {
            this.railway = railway;
            this.logician = logician;
        }

        public Node convert_expression(Pattern_Source source, Scope scope, Node previous = null)
        {
            switch (source.type)
            {
                case "block":
                    return create_block(source.patterns, scope);

                case "literal":
                    return create_literal(source, scope);

                case "path":
                    return new Reference_Path(create_path(source, scope));

                case "function":
                    return new Function_Call(source.text, previous, railway);

                case "array":
                    return create_array(source, scope);

                case "expression":
                    return process_expression(source, scope);

                case "int":
                    return new Literal_Value(int.Parse(source.text), Kind.Int);

                case "id":
                    var variable = scope.find(source.text);
                    if (variable != null)
                    {
                        return new Variable(source.text, variable);
                        //if (variable.rail == null)
                        //throw new Exception("variabcle rail cannot be null.");
                    }
                    else
                    {
                        Tie tie = scope.rail.get_tie_or_error(source.text);
                        return new Property_Reference(tie);
                        //if (tie.other_rail != null)
                        //    rail = tie.other_rail;
                    }
                //{
                //    Tie tie = scope.rail.get_tie_or_error(source.text);
                //    return new Property_Reference(tie);
                //}

                case "reference":
                    return source.patterns.Length < 2
                        ? convert_expression(source.patterns[0], scope)
                        : new Reference_Path(create_path(source, scope));
            }

            throw new Exception("Invalid block: " + source.type);
        }

        public Node convert_statement(Pattern_Source source, Scope scope, Signature type = null)
        {
            switch (source.type)
            {
                case "start":
                case "block":
                case "long_block":
                    return create_block(source.patterns, scope);

                //case "symbol":
                //return create_symbol(source, scope);
                case "new_scope":
                    return create_scope(source, scope);
                //case "create_node":
                //return create_node(source, scope);
                //case "if":
                //return if_statement(source, scope);

                case "constraint":
                    return constraint(source, scope);

                case "function_scope":
                    return function_scope(source, scope);

                case "create_group":
                    create_group(source, scope);
                    return null;

                case "group_scope":
                    var new_scope = new Scope(scope);
                    scope.group = logician.groups[source.patterns[2].text];

                    create_block(source.patterns[4].patterns, new_scope);
                    return null;

                    //case "weight":
                    //return weight(source, scope);
            }

            throw new Exception("Invalid block: " + source.type);
        }

        Node constraint(Pattern_Source source, Scope scope)
        {
            //var reference = Reference.from_scope(source.path, scope);
            var reference = create_path(source.patterns[0], scope);
            Node[] back_reference = null;
            var operator_name = source.patterns[2].text;
            if (new List<string> { "+=", "-=", "*=", "/=" }.Contains(operator_name))
            {
                //operator_name = operator_name.substring(0, operator_name.Count - 7);
                back_reference = reference;
            }
            var expression = create_path(source.patterns[4], scope);
            var lambda_array = source.patterns[5].patterns;
            var lambda_source = lambda_array != null && lambda_array.Length > 0 ? lambda_array[0] : null;
            var lambda = lambda_source != null
                ? create_lambda(lambda_source, scope, new List<Node[]> { reference, expression })
                : null;

            //return new Constraint(reference, expression, operator_name,
            //    lambda != null ? create_lambda(lambda, scope, new List<Node> { reference, expression }) : null
            //);
            var constraint = logician.create_constraint(reference, expression, operator_name, lambda, scope);

            return new Constraint_Wrapper(constraint);
        }

        Node create_block(Pattern_Source[] expressions, Scope scope)
        {
            var count = expressions.Length;
            if (count == 0)
                return new Block();

            var fields = expressions;

            if (count == 1)
            {
                return convert_statement(fields.First(), scope);
            }
            Block block = new Block();

            foreach (var expression in expressions)
            {
                block.children.Add(convert_statement(expression, scope));
            }

            return block;
        }

        Node create_literal(Pattern_Source source, Scope scope)
        {
            var type = get_type(source.value);
            //return new metahub.code.expressions.Literal(source.value, type);
            return new Literal_Value(source.value, Kind.unknown);
        }

        //Node[] to_path(Pattern_Source source, Scope scope)
        //{
        //    return create_path(source, scope).children;
        //}

        Node[] create_path(Pattern_Source source, Scope scope)
        {
            Rail rail = scope.rail;
            Node expression = null;
            List<Node> children = new List<Node>();
            var expressions = source.patterns;
            if (expressions == null || expressions.Length == 0)
            {
                return new Node[]
                    {
                        convert_expression(source, scope)
                    };
            }

            //if (expressions[0].type == "reference" && expressions[0].text != null 
            //    && rail.get_tie_or_null(expressions[0].text) == null
            //    && scope.find(expressions[0].text) == null)
            //{
            //    throw new Exception("Not supported.");
            //}
            Node previous = null;

            foreach (var item in expressions)
            {
                if (item.text == null && item.type == "reference")
                {
                    children.AddRange(create_path(item, scope));
                }
                else
                {
                    previous = convert_expression(item, scope, previous);
                    children.Add(previous);
                    var signature = previous.get_signature();
                    if (signature.rail != null)
                        scope = new Scope(scope) { rail = signature.rail };
                }
                /*     switch (item.type)
                     {
                         case "function":
                             previous = new Function_Call(item.text, railway);
                             //var info = Function_Call.get_function_info(item.name, hub);
                             //children.Add(new metahub.code.expressions.Function_Call(item.name, info, [], hub));
                             break;

                         case "id":
                         case "reference":
                             if (item.text != null)
                             {
                                 var variable = scope.find(item.text);
                                 if (variable != null)
                                 {
                                     previous = new Variable(item.text, variable);
                                     if (variable.rail == null)
                                         throw new Exception("variable rail cannot be null.");

                                     rail = variable.rail;
                                     //    throw new Exception("");
                                     //rail = variable.rail;
                                     //throw new Exception("Not implemented");
                                 }
                                 else
                                 {
                                     Tie tie = rail.get_tie_or_error(item.text);
                                     previous = new Property_Reference(tie);
                                     if (tie.other_rail != null)
                                         rail = tie.other_rail;
                                 }
                             }
                             else
                             {
                                 children.AddRange(create_path(item, scope));
                                 previous = null;
                             }
                             break;

                         case "array":
                             var items = (item).patterns;
                             Node token = null;
                             var sub_array = items.Select(i => convert_expression(i, token, scope)).ToArray();
                             previous = new Array_Expression(sub_array);
                             break;

                         case "int":
                             previous = new Literal_Value(int.Parse(item.text), Kind.Int);
                             break;

                         default:
                             throw new Exception("Invalid path token type: " + item.type);
                     }

                     if (previous != null)
                         children.Add(previous);
                 * */
            }

            return children.ToArray();
        }

        static Signature get_type(object value)
        {
            if (value is int)
            {
                return new Signature
                    {
                        type = Kind.unknown,
                        is_numeric = 1
                    };
            }

            if (value is float)
                return new Signature(Kind.Float);

            if (value is bool)
                return new Signature(Kind.Bool);

            if (value is string)
                return new Signature(Kind.String);

            throw new Exception("Could not find type.");
        }

        Node create_scope(Pattern_Source source, Scope scope)
        {
            var path = source.patterns[0].patterns;
            if (path.Length == 0)
                throw new Exception("Scope path is empty for node creation.");

            Node expression = null;
            Scope new_scope = new Scope();
            if (path.Length == 1 && path[0].text == "new")
            {
                //new_scope_definition.only_new = true;
                expression = convert_statement(source.patterns[2], new_scope);
                return new Scope_Expression(new_scope, new List<Node> { expression });
                //return new Scope_Expression(Node, new_scope_definition);
            }

            var rail = railway.resolve_rail_path(path.Select(t => t.text));// hub.schema.root_namespace.get_namespace(path);
            //var trellis = hub.schema.get_trellis(path[path.Count - 1], namespace);

            //if (rail != null) {
            new_scope.rail = rail;
            expression = convert_statement(source.patterns[2], new_scope);
            //return new Scope_Expression(Node, new_scope_definition);
            return new Scope_Expression(new_scope, new List<Node> { expression });
        }

        //Node weight (object source, Scope scope) {
        //return new Set_Weight(source.weight, convert_statement(source.statement, scope));
        //}

        Node create_array(Pattern_Source source, Scope scope)
        {
            var sub_array = source.patterns.Select(i => convert_expression(i, scope)).ToArray();
            return new Array_Expression(sub_array);
            //return new Block(source.patterns.Select((e) => convert_expression(e, scope)));
        }

        Lambda create_lambda(Pattern_Source source, Scope scope, List<Node[]> constraint_expressions)
        {
            var expressions = source.patterns[3].patterns;
            Scope new_scope = new Scope(scope);
            new_scope.is_map = true;
            var parameters = source.patterns[1].patterns;
            int i = 0;
            foreach (var parameter in parameters)
            {
                var expression = constraint_expressions[i];
                var path = expression;
                new_scope.variables[parameter.text] = get_path_signature(path);
                ++i;
            }

            return new Lambda(new_scope, parameters.Select((p) => new Parameter(p.text, null))
                , expressions.Select(e => convert_statement(e, new_scope))
            );
        }

        Node function_scope(Pattern_Source source, Scope scope)
        {
            var expression = create_path(source.patterns[0], scope);
            //var path = (Reference_Path)expression;
            //var token = path.children[path.children.Count - 2];
            var lambda = create_lambda(source.patterns[1], scope, new List<Node[]> { expression, expression });
            foreach (Constraint_Wrapper wrapper in lambda.expressions)
            {
                wrapper.constraint.caller = expression;
                logician.constraints.Add(wrapper.constraint);
            }
            return new Function_Scope(expression, lambda);
        }

        Node process_expression(Pattern_Source source, Scope scope)
        {
            if (source.patterns.Length < 2)
                return convert_expression(source.patterns[0], scope);

            //throw new Exception("Not implemented.");
            return new Operation_Node(source.text, source.patterns.Select(p=>convert_expression(p, scope)));

            //var rep_match = (Repetition_Match)match;
            //string op = rep_match.dividers[0].matches[1].get_data().text;

            //if (op == "|")
            //{
            //    var function_name = data.patterns.Last();
            //    data = ;
            //    var block = ((Parser_Block) function_name);

            //    return new Parser_Function_Call
            //        {
            //            type = "function",
            //            name = block.expressions[0].name,
            //            inputs = data.patterns.Take(data.patterns.Length - 1).ToArray()
            //        };
            //}
            //else
            //{
            //    return new Parser_Function_Call
            //        {
            //            type = "function",
            //            name = op,
            //            inputs = data
            //        };
            //}
        }

        static Signature get_path_signature(Node[] path)
        {
            var i = path.Length;
            while (--i >= 0)
            {
                var token = path[i] as Property_Reference;
                if (token == null)
                    continue;

                if (!token.tie.rail.trellis.is_value)
                    return token.tie.get_signature();
            }

            throw new Exception("Could not find signature.");
        }

        Constraint_Group create_group(Pattern_Source source, Scope scope)
        {
            var group = new Constraint_Group(source.patterns[4].text);
            logician.groups[group.name] = group;
            return group;
        }
    }
}