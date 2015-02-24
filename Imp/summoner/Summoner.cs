﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.expressions;
using metahub.jackolantern.expressions;

using parser;
using metahub.schema;

namespace metahub.imperative.summoner
{
    public partial class Summoner
    {
        private Overlord overlord;

        public Summoner(Overlord overlord)
        {
            this.overlord = overlord;
        }

        public void summon(Pattern_Source source)
        {
            foreach (var pattern in source.patterns)
            {
                process_namespace(pattern);
            }
        }

        private void process_namespace(Pattern_Source source)
        {
            var name = source.patterns[2].text;
            if (!overlord.realms.ContainsKey(name))
            {
                overlord.realms[name] = new Realm(name, overlord);
            }
            var realm = overlord.realms[name];
            var context = new Context(realm);
            var statements = source.patterns[6].patterns;

            foreach (var statement in statements)
            {
                process_dungeon1(statement, context);
            }

            foreach (var statement in statements)
            {
                process_dungeon2(statement, context);
            }
        }

        public void process_dungeon1(Pattern_Source source, Context context)
        {
            var name = source.patterns[2].text;
            var replacement_name = context.get_string_pattern(name);
            if (replacement_name != null)
                name = replacement_name;

            if (!context.realm.dungeons.ContainsKey(name))
            {
                var dungeon = context.realm.create_dungeon(name);
                var parent_dungeons = source.patterns[4].patterns;
                if (parent_dungeons.Length > 0)
                    dungeon.parent = overlord.get_dungeon(parent_dungeons[0].patterns[0].text);

                dungeon.generate_code();
            }
        }

        public Dungeon process_dungeon2(Pattern_Source source, Context context)
        {
            var name = source.patterns[2].text;

            var replacement_name = context.get_string_pattern(name);
            if (replacement_name != null)
                name = replacement_name;

            var statements = source.patterns[7].patterns;
            var dungeon = context.realm.dungeons[name];
            var dungeon_context = new Context(context) { dungeon = dungeon };
            foreach (var statement in statements)
            {
                process_dungeon_statement(statement, dungeon_context);
            }

            return dungeon;
        }

        private void process_dungeon_statement(Pattern_Source source, Context context)
        {
            switch (source.type)
            {
                case "abstract_function":
                    process_abstract_function(source, context);
                    break;

                case "function_definition":
                    process_function_definition(source, context);
                    break;

                case "property_declaration":
                    process_property_declaration(source, context);
                    break;
            }
        }

        private void process_abstract_function(Pattern_Source source, Context context)
        {
            var minion = context.dungeon.spawn_minion(
                source.patterns[2].text,
                source.patterns[5].patterns.Select(p => process_parameter(p, context)).ToList()
                );

            minion.is_abstract = true;

            var return_type = source.patterns[8];
            if (return_type.patterns.Length > 0)
                minion.return_type = parse_type(return_type.patterns[0], context);
        }

        private void process_function_definition(Pattern_Source source, Context context)
        {
            var name = source.patterns[1].text;
            var minion = context.dungeon.has_minion(name)
                          ? context.dungeon.summon_minion(name)
                          : context.dungeon.spawn_minion(
                              name,
                              source.patterns[4].patterns.Select(p => process_parameter(p, context)).ToList()
                                );
            var new_context = new Context(context) { scope = minion.scope };

            var return_type = source.patterns[7];
            if (return_type.patterns.Length > 0)
                minion.return_type = parse_type(return_type.patterns[0], context);

            minion.expressions = process_block(source.patterns[9], new_context);
        }

        private void process_property_declaration(Pattern_Source source, Context context)
        {
            var type_info = parse_type2(source.patterns[2], context);
            var portal_name = source.patterns[0].text;
            if (!context.dungeon.has_portal(portal_name))
                context.dungeon.add_portal(new Portal(portal_name, type_info));
        }

        private List<Expression> process_block(Pattern_Source source, Context context)
        {
            var result = new List<Expression>();
            foreach (var pattern in source.patterns)
            {
                var expression = process_statement(pattern, context);
                if (expression.type == Expression_Type.statements)
                {
                    var statements = (Statements)expression;
                    result.AddRange(statements.children);
                }
                else
                {
                    result.Add(expression);
                }
            }

            return result;
        }

        public Expression process_statement(Pattern_Source source, Context context)
        {
            switch (source.type)
            {
                case "assignment":
                    return process_assignment(source, context);

                case "expression":
                    return process_expression(source, context);

                case "if":
                    return new Flow_Control(Flow_Control_Type.If,
                        process_expression(source.patterns[4], context),
                        process_block(source.patterns[8], context)
                    );

                case "while":
                    return new Flow_Control(Flow_Control_Type.While, 
                        process_expression(source.patterns[4], context),
                        process_block(source.patterns[8], context)
                    );

                case "for":
                    return process_iterator(source, context);

                case "return":
                    return new Statement("return", source.patterns[1].patterns.Length == 0
                                                       ? null
                                                       : process_expression(source.patterns[1].patterns[0], context)
                        );

                case "declare_variable":
                    var symbol = context.scope.create_symbol(source.patterns[2].text,
                                                             parse_type2(source.patterns[4], context));
                    return new Declare_Variable(symbol, source.patterns[5].patterns.Length == 0
                                                            ? null
                                                            : process_expression(source.patterns[5].patterns[0], context)
                        );
                case "snippet_function":
                    {
                        return process_function_snippet(source, context);
                    }

                case "snippets":
                    var snippets = source.patterns.Select(p => process_statement(p, context)).ToList();
                    return new Statements(snippets);

                case "statements":
                    {
                        var expressions = process_block(source, context);
                        return expressions.Count == 1
                            ? expressions[0]
                            : new Statements(expressions);
                    }
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        private Expression process_expression(Pattern_Source source, Context context)
        {
            if (source.patterns.Length == 1)
                return process_expression_part(source.patterns[0], context);

            if (source.patterns.Length == 2)
            {
                var op = context.get_string_pattern(source.text) ?? source.text;
                return new Operation(op, source.patterns.Select(p => process_expression_part(p, context)));
            }

            throw new Exception("Not supported.");
        }

        private Expression process_expression_part(Pattern_Source source, Context context)
        {
            switch (source.type)
            {
                case "bool":
                    return new Literal(source.text == "true");

                case "int":
                    return new Literal(int.Parse(source.text));

                case "reference":
                    return process_reference(source, context);

                case "expression":
                    return process_expression(source, context);

                case "instantiate":
                    return process_instantiate(source, context);
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        private Expression process_reference(Pattern_Source source, Context context)
        {
            var dungeon = context.dungeon;
            Expression result = null;
            Expression last = null;
            var patterns = source.patterns[0].patterns;
            if (patterns.Length == 1)
            {
                if (patterns[0].text == "null")
                    return new Null_Value();
            }
            List<Expression> args = null;
            if (source.patterns[1].patterns.Length > 0)
            {
                args = source.patterns[1].patterns[0].patterns
                                                     .Select(p => process_expression(p, context))
                                                     .ToList();
            }

            foreach (var pattern in patterns)
            {
                var token = pattern.text;
                Expression array_access = pattern.patterns.Length > 0
                                              ? process_expression(pattern.patterns[0], context)
                                              : null;

                Portal portal = null;
                Expression next = null;

                var insert = context.get_expression_pattern(token);
                if (insert != null)
                {
                    next = insert;
                }
                else
                {
                    var symbol = context.scope.find_or_null(token);
                    if (symbol != null)
                    {
                        next = new Variable(symbol) { index = array_access };
                        var profession = next.get_profession();
                        dungeon = profession != null
                            ? profession.dungeon
                            : next.get_profession().dungeon;
                    }
                    else if (token == "this")
                    {
                        return new Self(dungeon);
                    }
                    else
                    {
                        if (dungeon != null)
                            portal = dungeon.get_portal_or_null(token);

                        if (portal != null)
                        {
                            next = new Portal_Expression(portal) { index = array_access };
                            dungeon = portal.other_dungeon;
                        }
                        else
                        {
                            var func = process_function_call(dungeon, token, result, last, args);
                            if (func != null)
                            {
                                if (func.type == Expression_Type.property_function_call)
                                {
                                    if (last.parent != null)
                                    {
                                        //last.parent.child = null;
                                        var last2 = last.parent;
                                        last = last.parent;
                                        last2.child = null;
                                    }
                                    else
                                    {
                                        return func;
                                    }
                                    next = func;
                                }
                                else
                                    return func;
                            }
                            else
                            {
                                throw new Exception("Invalid path token: " + token);
                            }
                        }
                    }
                }

                if (result == null)
                    result = next;
                else
                {
                    if (last.type == Expression_Type.property_function_call)
                        ((Property_Function_Call)last).args.Add(next);
                    else
                        last.child = next;
                }
                last = next.get_end();
            }

            return result;
        }

        private Expression process_function_call(Dungeon dungeon, string token, Expression result, Expression last,
                                                 List<Expression> args)
        {
            var minion = dungeon != null
                          ? dungeon.summon_minion(token, true)
                          : null;

            if (minion != null)
                return new Class_Function_Call(minion, result, args);

            if (Minion.platform_specific_functions.Contains(token))
            {
                if (token == "add" || token == "setter")
                    return new Property_Function_Call(Property_Function_Type.set, ((Portal_Expression)last).portal,
                                                      args);

                return new Platform_Function(token, result, args);
            }

            return null;
        }

        private Parameter process_parameter(Pattern_Source source, Context context)
        {
            return new Parameter(new Symbol(source.patterns[1].text, new Profession(Kind.unknown), null));
        }

        private Profession parse_type(Pattern_Source source, Context context)
        {
            source = source.patterns[2];
            var text = source.patterns.Last().text;

            if (source.patterns.Length == 1)
            {
                switch (text)
                {
                    case "bool":
                        return new Profession(Kind.Bool);
                    case "string":
                        return new Profession(Kind.String);
                    case "float":
                        return new Profession(Kind.Float);
                    case "int":
                        return new Profession(Kind.Int);
                }
            }

            Realm realm = null;
            for (var i = 0; i < source.patterns.Length - 1; ++i)
            {
                if (realm == null)
                    realm = overlord.realms[source.patterns[i].text];
                else
                    throw new Exception("embedded namespaces are not supported yet.");
            }

            if (realm == null)
                realm = context.realm;

            if (realm.dungeons.ContainsKey(text))
                return new Profession(Kind.reference, realm.dungeons[text]);

            var dungeon = overlord.get_dungeon(text);
            if (dungeon != null)
                return new Profession(Kind.reference, dungeon);

            throw new Exception("Invalid type: " + text + ".");
        }

        private Profession parse_type2(Pattern_Source source, Context context)
        {
            var path = source.patterns[2].patterns.Select(p => p.text).ToArray();
            var is_list = source.patterns.Length > 1 && source.patterns[3].patterns.Length > 0;
            return parse_type2(path, context, is_list);
        }

        private Profession parse_type2(string[] path, Context context, bool is_list = false)
        {
            var text = path.Last();
            if (path.Length == 1)
            {
                switch (text)
                {
                    case "bool":
                        return new Profession(Kind.Bool) { is_list = is_list };
                    case "string":
                        return new Profession(Kind.String) { is_list = is_list };
                    case "float":
                        return new Profession(Kind.Float) { is_list = is_list };
                    case "int":
                        return new Profession(Kind.Int) { is_list = is_list };
                }

                var profession = context.get_profession_pattern(text);
                if (profession != null)
                {
                    var result = profession.clone();
                    result.is_list = is_list;
                    return result;
                }
            }

            Realm realm = null;
            for (var i = 0; i < path.Length - 1; ++i)
            {
                if (realm == null)
                    realm = overlord.realms[path[i]];
                else
                    throw new Exception("embedded namespaces are not supported yet.");
            }

            if (realm == null)
                realm = context.realm;

            if (realm.dungeons.ContainsKey(text))
                return new Profession(Kind.reference, realm.dungeons[text]) { is_list = is_list };

            var dungeon = overlord.get_dungeon(text);
            if (dungeon != null)
                return new Profession(Kind.reference, dungeon);

            throw new Exception("Invalid type: " + text + ".");
        }

        private Expression process_assignment(Pattern_Source source, Context context)
        {
            var reference = process_reference(source.patterns[0], context);
            var expression = process_expression(source.patterns[4], context);
            var op = source.patterns[2].text;
            var last = reference.get_end();

            if (last.type == Expression_Type.portal && op != "@=")
            {
                var portal_expression = (Portal_Expression)last;
                var portal = portal_expression.portal;
                if (portal.type != Kind.list && op != "=")
                {
                    expression = Minion.operation(op[0].ToString(), reference.clone(), expression);
                }
                var args = new List<Expression> {expression};

                // The setter absorbs the portal token, so remove it from the reference.
                if (last == reference)
                {
                    reference = null;
                }
                else
                {
                    last.parent.child = null;
                    last.parent = null;
                }

                // Check for origin parameter
                if (portal.setter != null && portal.setter.parameters.Count > 1)
                {
                    args.Add(new Self(context.dungeon));
                }

                return new Property_Function_Call(Property_Function_Type.set, portal, args) { reference = reference };
            }

            // @= forces direct assignment without setters
            if (op == "@=")
                op = "=";

            return new Assignment(
                reference,
                op,
                expression
                );
        }

        public Expression process_iterator(Pattern_Source source, Context context)
        {
            var reference = process_expression_part(source.patterns[10], context);
            var profession = reference.get_end().get_profession().get_reference();
            var symbol = context.scope.create_symbol(source.patterns[6].text, profession);
            context.scope.add_map(symbol.name, c => new Variable(symbol));
            return new Iterator(symbol,
                                reference, process_block(source.patterns[14], context)
                );
        }

        private Expression process_instantiate(Pattern_Source source, Context context)
        {
            var type = parse_type2(new[] { source.patterns[2].text }, context);
            return new Instantiate(type.dungeon);
        }

        //private Expression process_snippet_functions(Pattern_Source source, Context context)
        //{
        //    var functions = source.patterns.Select(p => process_function_snippet(p, context));
        //    return new Statements(functions.ToList());
        //}

        private Expression process_function_snippet(Pattern_Source source, Context context)
        {
            var name = source.patterns[1].text;
            var body = source.patterns[9];
            var parameters = source.patterns[4].patterns.Select(p => p.text).ToArray();
            return new Snippet(name, body, parameters);
        }

    }
}