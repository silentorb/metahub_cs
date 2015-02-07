using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;
using metahub.logic.nodes;
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

        public Node convert_expression(Pattern_Source source, Logic_Scope scope, Node previous = null)
        {
            switch (source.type)
            {
                case "block":
                    return create_block(source.patterns, scope);

                case "literal":
                    return create_literal(source, scope);

                case "path":
                    return process_path(source, scope);

                case "function":
                    return process_function_call(source, previous, scope);

                case "array":
                    return create_array(source, scope);

                case "expression":
                    return process_expression(source, scope);

                case "bool":
                    return new Literal_Value(bool.Parse(source.text), Kind.Bool);

                case "int":
                    return new Literal_Value(int.Parse(source.text), Kind.Int);

                case "complex_token":
                    return process_complex_token(source, scope);

                case "lambda":
                    return create_lambda(source, scope);
              
                case "id":
                    if (source.text == "null")
                        return new Null_Node();

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
                        : process_path(source, scope);
            }

            throw new Exception("Invalid expression type: " + source.type);
        }


        public Node convert_expression2(Pattern_Source source, Logic_Scope scope, Node previous = null)
        {
            switch (source.type)
            {
                case "block":
                    return create_block(source.patterns, scope);

                case "literal":
                    return create_literal(source, scope);

                case "path":
                    return process_path(source, scope);

                case "function":
                    return process_function_call(source, previous, scope);

                case "array":
                    return create_array(source, scope);

                case "expression":
                    return process_expression2(source, scope);

                case "bool":
                    return new Literal_Value(bool.Parse(source.text), Kind.Bool);

                case "int":
                    return new Literal_Value(int.Parse(source.text), Kind.Int);

                case "complex_token":
                    return process_complex_token(source, scope);

                case "lambda":
                    return create_lambda2(source, scope);

                case "id":
                    if (source.text == "null")
                        return new Null_Node();

                    var variable = scope.find(source.text);
                    if (variable != null)
                    {
                        return new Variable(source.text, variable);
                        //if (variable.rail == null)
                        //throw new Exception("variable rail cannot be null.");
                    }
                    else
                    {
                        Tie tie = scope.rail.get_tie_or_error(source.text);
                        var result = new Property_Reference(tie);
                        var input = previous ?? scope.scope_node;
                        if (input == null)
                           throw new Exception("Could not find input node.");

                        result.connect_input(input);
                        return result;
                        //if (tie.other_rail != null)
                        //    rail = tie.other_rail;
                    }
                //{
                //    Tie tie = scope.rail.get_tie_or_error(source.text);
                //    return new Property_Reference(tie);
                //}

                case "reference":
                    return source.patterns.Length < 2
                        ? convert_expression2(source.patterns[0], scope)
                        : process_path2(source, scope);
            }

            throw new Exception("Invalid expression type: " + source.type);
        }

        public Node convert_statement(Pattern_Source source, Logic_Scope scope, Signature type = null)
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

                case "expression":
                    return process_expression(source, scope);

                //case "function_scope":
                //    return function_scope(source, scope);

                case "create_group":
                    create_group(source, scope);
                    return null;

                case "group_scope":
                    var new_scope = new Logic_Scope(scope);
                    scope.group = logician.groups[source.patterns[2].text];

                    create_block(source.patterns[4].patterns, new_scope);
                    return null;

                    //case "weight":
                    //return weight(source, scope);
            }

            throw new Exception("Invalid block: " + source.type);
        }

        Node constraint(Pattern_Source source, Logic_Scope scope)
        {
            //var reference = Reference.from_scope(source.path, scope);
            //Node back_reference = null;
            var operator_name = source.patterns[2].text;

            //if (operator_name == "@=" || operator_name == ">")
            {
                return new_constraint(source, scope);
            }

            var reference = process_path(source.patterns[0], scope);

            //if (new List<string> { "+=", "-=", "*=", "/=" }.Contains(operator_name))
            //{
            //    //operator_name = operator_name.substring(0, operator_name.Count - 7);
            //    //back_reference = reference;
            //}
            var expression = process_path(source.patterns[4], scope);
            //var lambda_array = source.patterns[5].patterns;
            //var lambda_source = lambda_array != null && lambda_array.Length > 0 ? lambda_array[0] : null;
            //var lambda = lambda_source != null
            //    ? create_lambda(lambda_source, scope, new List<Node[]> { reference, expression })
            //    : null;
            Lambda lambda = null;

            //return new Constraint(reference, expression, operator_name,
            //    lambda != null ? create_lambda(lambda, scope, new List<Node> { reference, expression }) : null
            //);
            var constraint = logician.create_constraint(reference, expression, operator_name, lambda, scope);

            return new Constraint_Wrapper(constraint);
        }

        private Node new_constraint(Pattern_Source source, Logic_Scope scope)
        {
            var new_scope = new Logic_Scope(scope) { scope_node = new Scope_Node(scope.rail) };

            var name = source.patterns[2].text;
            return logician.call(name, new List<Node>
                {
                    process_path2(source.patterns[0], new_scope),
                    process_path2(source.patterns[4], new_scope)
                });
        }

        Node create_block(Pattern_Source[] expressions, Logic_Scope scope)
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

        Node create_literal(Pattern_Source source, Logic_Scope scope)
        {
            var type = get_type(source.value);
            //return new metahub.code.expressions.Literal(source.value, type);
            return new Literal_Value(source.value, Kind.unknown);
        }

        Node process_path2(Pattern_Source source, Logic_Scope scope)
        {
            var token_scope = scope;
            Rail rail = token_scope.rail;
            Node expression = null;
            var expressions = source.patterns;
            if (expressions == null || expressions.Length == 0)
            {
                return convert_expression2(source, token_scope);
            }

            Node previous = null;

            foreach (var item in expressions)
            {
                Node current = null;
                if (item.text == null && item.type == "reference")
                {
                    current = process_path2(item, token_scope);
                }
                else
                {
                    current = convert_expression2(item, token_scope, previous);
                    var signature = current.get_signature();
                    if (signature.rail != null)
                        token_scope = new Logic_Scope(scope) { rail = signature.rail };
                }

                if (previous == null)
                {
                    //current.connect_input(scope.scope_node);
                }
                else
                {
                    previous.connect_output(current);
                }
                
                previous = current;
            }

            return previous;
        }

        Node process_path(Pattern_Source source, Logic_Scope scope)
        {
            var token_scope = scope;
            Rail rail = token_scope.rail;
            Node expression = null;
            Node result = null;
            var expressions = source.patterns;
            if (expressions == null || expressions.Length == 0)
            {
                return convert_expression(source, token_scope);
            }

            Node previous = null;

            foreach (var item in expressions)
            {
                Node current = null;
                if (item.text == null && item.type == "reference")
                {
                    current = process_path(item, token_scope);
                }
                else
                {
                    current = convert_expression(item, token_scope, previous);
                    var signature = current.get_signature();
                    if (signature.rail != null)
                        token_scope = new Logic_Scope(scope) { rail = signature.rail };
                }

                if (result == null)
                {
                    result = current;
                }
                else
                {
                    if (current.type == Node_Type.function_call && current.GetType() == typeof(Function_Call2))
                    {
                        result = current;
                    }
                    else
                    {
                        previous.connect_input(current);
                    }
                }

                previous = current;
            }

            return result;
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

        Node create_scope(Pattern_Source source, Logic_Scope scope)
        {
            var path = source.patterns[0].patterns;
            if (path.Length == 0)
                throw new Exception("Scope path is empty for node creation.");

            Node expression = null;
            Logic_Scope new_scope = new Logic_Scope();
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

        Node create_array(Pattern_Source source, Logic_Scope scope)
        {
            var sub_array = source.patterns.Select(i => convert_expression(i, scope)).ToArray();
            return new Array_Expression(sub_array);
            //return new Block(source.patterns.Select((e) => convert_expression(e, scope)));
        }

        Node process_complex_token(Pattern_Source source, Logic_Scope scope)
        {
            var reference = convert_expression(source.patterns[0], scope);
            if (source.patterns[1].patterns.Length > 0)
            {
                throw new Exception();
            }

            return reference;
        }

        Lambda create_lambda(Pattern_Source source, Logic_Scope scope)
        {
            var expressions = source.patterns[2].patterns;
            Logic_Scope new_scope = new Logic_Scope(scope);
            new_scope.is_map = true;
            var parameters = source.patterns[0].patterns;
            int i = 0;
            foreach (var parameter in parameters)
            {
                var signature = scope.parameters[0].parameters[i];
                new_scope.variables[parameter.text] = signature;
                ++i;
            }

            throw new Exception("No longer supported.");
            //return new Lambda(new_scope, parameters.Select(p => new Parameter(p.text, null))
            //    , expressions.Select(e => ((Constraint_Wrapper)convert_statement(e, new_scope)).constraint)
            //);
        }

        Lambda create_lambda2(Pattern_Source source, Logic_Scope scope)
        {
            var expressions = source.patterns[2].patterns;
            Logic_Scope new_scope = new Logic_Scope(scope);
            new_scope.is_map = true;
            var parameters = source.patterns[0].patterns;
            int i = 0;
            foreach (var parameter in parameters)
            {
                var signature = scope.parameters[0].parameters[i];
                new_scope.variables[parameter.text] = signature;
                ++i;
            }

            return new Lambda(new_scope, parameters.Select(p => new Parameter(p.text, null))
                , expressions.Select(e => (Function_Call2)convert_statement(e, new_scope))
            );
        }

        Node process_expression(Pattern_Source source, Logic_Scope scope)
        {
            if (source.patterns.Length < 2)
                return convert_expression(source.patterns[0], scope);

            return new Function_Call2(source.text, source.patterns.Select(p=>convert_expression(p, scope)), true);
        }

        Node process_expression2(Pattern_Source source, Logic_Scope scope)
        {
            if (source.patterns.Length < 2)
                return convert_expression2(source.patterns[0], scope);

            return new Function_Call2(source.text, source.patterns.Select(p => convert_expression2(p, scope)), true);
        }

        //Node process_function_call(Pattern_Source source, Node previous, Logic_Scope scope)
        //{
        //    var name = source.text ?? source.patterns[0].text;
        //    //if (name == "contains" || name == "cross")
        //        return process_function_call3(source, previous, scope);

        //    //var function_scope = new Logic_Scope(scope.parent) { constraint_scope = new Constraint_Scope(name, new [] { previous })};
        //    //var function_signature = prepare_function_scope_signature(name, function_scope, previous);
  
        //    //var args = new Node[] {previous};
        //    //if (source.patterns != null)
        //    //{
        //    //    args = args.Concat(source.patterns[1].patterns[0].patterns
        //    //        .Select(p => convert_expression(p, function_scope))
        //    //        ).ToArray();
        //    //}
        //    //return new Function_Call(name, args, railway, function_signature);
            
        //}

        private Node process_function_call(Pattern_Source source, Node previous, Logic_Scope scope)
        {
            var name = source.text ?? source.patterns[0].text;
            var args = source.patterns != null ? source.patterns[1].patterns[0].patterns : null;
            return process_function_call2(name, args, previous, scope);
        }

        private Node process_function_call2(string name, Pattern_Source[] args, Node previous, Logic_Scope scope)
        {
            var function_scope = new Logic_Scope(scope.parent) { 
                constraint_scope = new Constraint_Scope(name, new [] { previous }),
                scope_node = new Scope_Node(scope.parent.rail)
            };
            var function_signature = prepare_function_scope_signature(name, function_scope, previous);
  
            var inputs = new Node[] { previous };
            if (args != null)
            {
                inputs = inputs.Concat(args.Select(p => convert_expression2(p, function_scope))).ToArray();
            }

            return logician.call(name, inputs, scope);
        }

        Signature prepare_function_scope_signature(string name, Logic_Scope function_scope, Node previous)
        {
            var func = railway.root_region.functions[name];
            var previous_signature = previous.get_signature();
            var function_signature = func.get_signature(previous_signature).clone();
            function_scope.parameters = function_signature.parameters.Skip(1).ToArray();
            if (previous.type == Node_Type.property)
            {
                var previous_property = (Property_Reference)previous;
                foreach (var parameter in function_scope.parameters)
                {
                    if (parameter.parameters != null)
                    {
                        foreach (var parameter2 in parameter.parameters)
                        {
                            if (parameter2.type == Kind.reference || parameter2.type == Kind.list)
                                parameter2.rail = previous_property.tie.other_rail;
                        }
                    }
                }
            }

            return function_signature;
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

        Constraint_Group create_group(Pattern_Source source, Logic_Scope scope)
        {
            var group = new Constraint_Group(source.patterns[4].text);
            logician.groups[group.name] = group;
            return group;
        }
    }
}