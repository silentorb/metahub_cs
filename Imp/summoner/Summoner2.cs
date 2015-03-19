using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.Properties;
using imperative.schema;
using imperative.expressions;
using metahub.jackolantern.expressions;

using metahub.schema;
using runic.lexer;
using runic.parser;
using Parser = runic.parser.Parser;

namespace imperative.summoner
{
    public class Summoner2
    {
        private Overlord overlord;
        private static Lexer lexer;
        private static Parser parser;

        public Summoner2(Overlord overlord)
        {
            this.overlord = overlord;
        }

        public void summon(Legend source)
        {
            summon((Group_Legend)source);
        }

        public void summon(Group_Legend source)
        {
            ack(source, process_dungeon1);
            ack(source, process_dungeon2);
            ack(source, process_dungeon3);
        }

        public void summon_many(IEnumerable<Legend> sources)
        {
            foreach (Group_Legend source in sources)
            {
                ack(source, process_dungeon1);
            }

            foreach (Group_Legend source in sources)
            {
                ack(source, process_dungeon2);
            }

            foreach (Group_Legend source in sources)
            {
                ack(source, process_dungeon3);
            }
        }

        void ack(Group_Legend source,
            Func<Legend, Summoner_Context, Dungeon> second)
        {
            foreach (var pattern in source.children)
            {
                if (pattern.type == "namespace_statement")
                {
                    var context = create_realm_context(pattern);
                    var statements = pattern.children[1].children;
                    process_namespace(statements, context, second);
                }
                else if (pattern.type == "class_statement")
                {
                    var context = new Summoner_Context(overlord.realms[""]);
                    second(pattern, context);
                }
                else
                {
                    throw new Exception("Not supported.");
                }
            }
        }

        private Summoner_Context create_realm_context(Legend source)
        {
            var name = source.children[0].text;
            if (!overlord.realms.ContainsKey(name))
            {
                overlord.realms[name] = new Realm(name, overlord);
            }
            var realm = overlord.realms[name];
            var context = new Summoner_Context(realm);

            return context;
        }

        private void process_namespace(IEnumerable<Legend> statements, Summoner_Context context,
            Func<Legend, Summoner_Context, Dungeon> dungeon_step)
        {
            foreach (var statement in statements)
            {
                dungeon_step(statement, context);
            }
        }

        public Dungeon process_dungeon1(Legend source, Summoner_Context context)
        {
            var name = source.children[1].text;
            var replacement_name = context.get_string_pattern(name);
            if (replacement_name != null)
                name = replacement_name;

            if (!context.realm.dungeons.ContainsKey(name))
            {
                var dungeon = context.realm.create_dungeon(name);
                if (source.children[0].children.Count > 0)
                    dungeon.is_abstract = source.children[0].children.Any(p => p.text == "abstract");

                var parent_dungeons = source.children[2].children;
                if (parent_dungeons.Count > 0)
                    dungeon.parent = overlord.get_dungeon(parent_dungeons[0].children[0].text);

                dungeon.generate_code();
                return dungeon;
            }

            return null;
        }

        public Dungeon process_dungeon2(Legend source, Summoner_Context context)
        {
            var name = source.children[1].text;

            var replacement_name = context.get_string_pattern(name);
            if (replacement_name != null)
                name = replacement_name;

            var statements = source.children[3].children;
            var dungeon = context.realm.dungeons[name];
            var dungeon_context = new Summoner_Context(context) { dungeon = dungeon };
            foreach (var statement in statements)
            {
                process_dungeon_statement(statement, dungeon_context, true);
            }

            return dungeon;
        }

        public Dungeon process_dungeon3(Legend source, Summoner_Context context)
        {
            var name = source.children[1].text;

            var replacement_name = context.get_string_pattern(name);
            if (replacement_name != null)
                name = replacement_name;

            var statements = source.children[3].children;
            var dungeon = context.realm.dungeons[name];
            var dungeon_context = new Summoner_Context(context) { dungeon = dungeon };
            foreach (var statement in statements)
            {
                process_dungeon_statement(statement, dungeon_context);
            }

            return dungeon;
        }

        private void process_dungeon_statement(Legend source, Summoner_Context context, bool as_stub = false)
        {
            switch (source.type)
            {
                //                case "abstract_function":
                //                    if (as_stub)
                //                        process_abstract_function(source, context);
                //                    break;

                case "function_definition":
                    process_function_definition(source, context, as_stub);
                    break;

                case "property_declaration":
                    process_property_declaration(source, context, as_stub);
                    break;
            }
        }

        //        private void process_abstract_function(Legend source, Context context)
        //        {
        //            var minion = context.dungeon.spawn_minion(
        //                source.children[0].text,
        //                source.children[3].children.Select(p => process_parameter(p, context)).ToList()
        //                );
        //
        //            minion.is_abstract = true;
        //
        //            var return_type = source.children[6];
        //            if (return_type.children.Length > 0)
        //                minion.return_type = parse_type(return_type.children[0], context);
        //        }

        private void process_function_definition(Legend source, Summoner_Context context, bool as_stub = false)
        {
            var parts = source.children;
            var name = parts[1].text;
            var minion = context.dungeon.has_minion(name)
                         ? context.dungeon.summon_minion(name)
                         : context.dungeon.spawn_minion(
                             name,
                             parts[2].children.Select(p => process_parameter(p, context)).ToList()
                               );

            var new_context = new Summoner_Context(context) { scope = minion.scope };

            if (as_stub)
            {
                var return_type = parts[3];
                if (return_type.children.Count > 0)
                    minion.return_type = parse_type(return_type.children[0], context);
            }
            else
            {
                if (parts[4].children.Count == 0)
                    minion.is_abstract = true;
                else
                    minion.add_to_block(process_block(parts[4].children[0], new_context));
            }
        }

        private void process_property_declaration(Legend source, Summoner_Context context, bool as_stub = false)
        {
            if (!as_stub)
                return;

            var type_info = parse_type2(source.children[1].children[0], context);
            var portal_name = source.children[0].text;
            if (!context.dungeon.has_portal(portal_name))
                context.dungeon.add_portal(new Portal(portal_name, type_info));
        }

        private List<Expression> process_block(Legend source, Summoner_Context context)
        {
            var result = new List<Expression>();
            foreach (var pattern in source.children)
            {
                var expression = process_statement(pattern, context);
                if (expression.type == Expression_Type.statements)
                {
                    var statements = (Block)expression;
                    result.AddRange(statements.body);
                }
                else
                {
                    result.Add(expression);
                }
            }

            return result;
        }

        public Expression process_statement(Legend source, Summoner_Context context)
        {
            var parts = source.children;

            switch (source.type)
            {
                case "assignment":
                    return process_assignment(parts, context);

                case "expression_part":
                    return process_expression(source, context);

                case "if_statement":
                    return new Flow_Control(Flow_Control_Type.If,
                        process_expression(parts[0], context),
                        process_block(parts[1], context)
                    );

                case "while_statement":
                    return new Flow_Control(Flow_Control_Type.While,
                        process_expression(parts[0], context),
                        process_block(parts[1], context)
                    );

                case "for_statement":
                    return process_iterator(parts, context);

                case "return_statement":
                    return new Statement("return", parts[0].children.Count == 0
                        ? null
                        : process_expression(parts[0].children[0], context)
                    );

                case "declare_variable":
                    return process_declare_variable(parts, context);

                case "snippet_function":
                    {
                        return process_function_snippet(parts, context);
                    }

                case "snippets":
                    var snippets = parts.Select(p => process_statement(p, context)).ToList();
                    return new Block(snippets);

                case "statements":
                    {
                        var expressions = process_block(source, context);
                        return expressions.Count == 1
                            ? expressions[0]
                            : new Block(expressions);
                    }
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        private Expression process_expression(Legend legend, Summoner_Context context)
        {
            var group = (Group_Legend) legend;
            var children = group.children;

            if (children.Count == 1)
                return process_expression_part(children[0], context);

            if (children.Count == 2)
            {
                var op = group.dividers[0].text;
                return new Operation(op, children.Select(p => process_expression_part(p, context)));
            }

            throw new Exception("Not supported.");
        }

        Expression process_declare_variable(List<Legend> parts, Summoner_Context context)
        {
            Profession profession;
            var expression_pattern = parts[2].children.Count == 0
                ? null
                : process_expression(parts[2].children[0], context);

            if (parts[1].children.Count > 0)
            {
                profession = parse_type2(parts[1], context);
            }
            else
            {
                if (expression_pattern == null)
                    throw new Exception("Cannot discern variable type.");

                profession = expression_pattern.get_profession();
            }

            var symbol = context.scope.create_symbol(parts[0].text, profession);
            return new Declare_Variable(symbol, expression_pattern);
        }

        private Expression process_expression_part(Legend source, Summoner_Context context)
        {
            var parts = source.children;

            switch (source.type)
            {
                case "bool":
                    return new Literal(source.text == "true");

                case "int":
                    return new Literal(int.Parse(source.text));

                case "float":
                    return new Literal(float.Parse(source.text));

                case "string":
                    return new Literal(source.text);

                case "null":
                    return new Null_Value();

                case "reference":
                    return process_reference(source, context);

                case "expression":
                    return process_expression(source, context);

                case "instantiate":
                    return process_instantiate(parts, context);

                case "lambda":
                    return process_lambda(parts, context);
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        private Expression process_reference(Legend source, Summoner_Context context)
        {
            var dungeon = context.dungeon;
            Expression result = null;
            Expression last = null;
            var patterns = source.children[0].children;
            if (patterns.Count == 1)
            {
                if (patterns[0].text == "null")
                    return new Null_Value();
            }
            List<Expression> args = null;
            if (source.children[1].children.Count > 0)
            {
                args = source.children[1].children[0].children
                                                     .Select(p => process_expression(p, context))
                                                     .ToList();
            }

            var index = -1;
            foreach (var pattern in patterns)
            {
                ++index;
                var token = pattern.children[0].text;
                Expression array_access = pattern.children[1].children.Count > 0
                                              ? process_expression(pattern.children[1].children[0], context)
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
                    if (token == "this")
                    {
                        return new Self(dungeon);
                    }

                    var symbol = context.scope.find_or_null(token);
                    if (symbol != null)
                    {
                        if (index == patterns.Count - 1 && symbol.profession.type == Kind.function)
                        {
                            next = new Dynamic_Function_Call(symbol.name, null, args);
                        }
                        else
                        {
                            next = new Variable(symbol) { index = array_access };
                            var profession = next.get_profession();
                            dungeon = profession != null
                                ? profession.dungeon
                                : next.get_profession().dungeon;
                        }
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
                                        last2.next = null;
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
                                dungeon = overlord.get_dungeon(token);
                                if (dungeon != null)
                                {
                                    next = new Profession_Expression(new Profession(Kind.reference, dungeon));
                                }
                                else
                                {
                                    throw new Exception("Invalid path token: " + token);
                                }
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
                        last.next = next;
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
                return new Method_Call(minion, result, args);

            if (Minion.platform_specific_functions.Contains(token))
            {
                if (token == "add" || token == "setter")
                    return new Property_Function_Call(Property_Function_Type.set, ((Portal_Expression)last).portal,
                                                      args);

                return new Platform_Function(token, result, args);
            }

            return null;
        }

        private Parameter process_parameter(Legend source, Summoner_Context context)
        {
            var type = source.children[1].children.Count > 0
                ? parse_type2(source.children[1].children[0].children[0], context)
                : new Profession(Kind.unknown);

            return new Parameter(new Symbol(source.children[0].text, type, null));
        }

        private Profession parse_type(Legend source, Summoner_Context context)
        {
            source = source.children[2];
            var text = source.children.Last().text;

            if (source.children.Count == 1)
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
            for (var i = 0; i < source.children.Count - 1; ++i)
            {
                if (realm == null)
                    realm = overlord.realms[source.children[i].text];
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

        private Profession parse_type2(Legend source, Summoner_Context context)
        {
            var path = source.children.Select(p => p.text).ToArray();
            var is_list = source.children.Count > 1 && source.children[1].children.Count > 0;
            return parse_type2(path, context, is_list);
        }

        private Profession parse_type2(string[] path, Summoner_Context context, bool is_list = false)
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
                {
                    var realm_name = path[i];
                    if (!overlord.realms.ContainsKey(realm_name))
                        throw new Exception("Could not find realm: " + realm_name + ".");

                    realm = overlord.realms[realm_name];
                }
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

        private Expression process_assignment(List<Legend> parts, Summoner_Context context)
        {
            var reference = process_reference(parts[0], context);
            var expression = process_expression(parts[2], context);
            var op = parts[1].text;
            var last = reference.get_end();
            if (reference != null && reference.type == Expression_Type.operation)
                throw new Exception("Cannot call function on operation.");

            if (last.type == Expression_Type.portal && op != "@=")
            {
                var portal_expression = (Portal_Expression)last;
                var portal = portal_expression.portal;
                if (portal.type != Kind.list && op != "=")
                {
                    expression = Minion.operation(op[0].ToString(), reference.clone(), expression);
                }
                var args = new List<Expression> { expression };

                // The setter absorbs the portal token, so remove it from the reference.
                if (last == reference)
                {
                    reference = null;
                }
                else
                {
                    //if (last.parent.type == Expression_Type.operation || last.type== Expression_Type.operation)
                    //    throw new Exception();
                    //last.parent.child = null;
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

        public Expression process_iterator(List<Legend> parts, Summoner_Context context)
        {
            var reference = process_expression_part(parts[1], context);
            var profession = reference.get_end().get_profession().get_reference();
            var symbol = context.scope.create_symbol(parts[0].text, profession);
            context.scope.add_map(symbol.name, c => new Variable(symbol));
            return new Iterator(symbol,
                                reference, process_block(parts[2], context)
                );
        }

        private Expression process_instantiate(List<Legend> parts, Summoner_Context context)
        {
            var type = parse_type2(parts[0], context);
            var args = parts[1].children.Select(p => process_expression(p, context));
            return new Instantiate(type.dungeon, args);
        }

        private Expression process_function_snippet(List<Legend> parts, Summoner_Context context)
        {
            var name = parts[0].text;
            var body = parts[2];
            var parameters = parts[1].children.Select(p => p.text).ToArray();
            return new Snippet2(name, body, parameters);
        }

        private Expression process_lambda(List<Legend> parts, Summoner_Context context)
        {
            var parameters = parts[0].children.Select(p => process_parameter(p, context));
            var minion = new Ethereal_Minion(parameters, context.scope);
            var new_context = new Summoner_Context(context) { scope = minion.scope };
            var block = process_block(parts[1], new_context);
            minion.expressions.AddRange(block);
            minion.return_type = new Profession(Kind.none);

            return new Anonymous_Function(minion);
        }

        public static List<Rune> read_runes(string input)
        {
            if (lexer == null)
                lexer = new Lexer(Resources.imp_lexer);

            return lexer.read(input);
        }

        public static Legend translate_runes(List<Rune> runes)
        {
            if (parser == null)
                parser = new Parser(lexer, Resources.imp2_grammar);

            return parser.read(runes);
        }
    }
}
