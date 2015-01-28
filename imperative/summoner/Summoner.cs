using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.parser;
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

        void process_namespace(Pattern_Source source)
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

            dungeon = context.realm.create_dungeon(name);
  
            var dungeon_context = new Context(context) { dungeon = dungeon };
            foreach (var statement in statements)
            {
                process_dungeon_statement(statement, dungeon_context);
            }

            return dungeon;
        }

        void process_dungeon_statement(Pattern_Source source, Context context)
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

        void process_abstract_function(Pattern_Source source, Context context)
        {
            var imp = context.dungeon.spawn_imp(
                source.patterns[2].text,
                source.patterns[5].patterns.Select(p => process_parameter(p, context)).ToList()
            );

            imp.is_abstract = true;

            var return_type = source.patterns[8];
            if (return_type.patterns.Length > 0)
                imp.return_type = parse_type(return_type.patterns[0], context);
        }

        void process_function_definition(Pattern_Source source, Context context)
        {
            var imp = context.dungeon.spawn_imp(
                         source.patterns[1].text,
                         source.patterns[4].patterns.Select(p => process_parameter(p, context)).ToList()
                     );
            var new_context = new Context(context) { scope = imp.scope };

            var return_type = source.patterns[7];
            if (return_type.patterns.Length > 0)
                imp.return_type = parse_type(return_type.patterns[0], context);

            imp.expressions = process_block(source.patterns[9], new_context);
        }

        void process_property_declaration(Pattern_Source source, Context context)
        {
            var type_info = parse_type2(source.patterns[2], context);
            context.dungeon.add_portal(new Portal(source.patterns[0].text, type_info));
        }

        List<Expression> process_block(Pattern_Source source, Context context)
        {
            return source.patterns.Select(p => process_statement(p, context)).ToList();
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
                    return new Flow_Control(Flow_Control_Type.If, process_expression(source.patterns[4], context),
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
                    var symbol = context.scope.create_symbol(source.patterns[2].text, parse_type(source.patterns[4], context));
                    return new Declare_Variable(symbol, source.patterns[5].patterns.Length == 0
                        ? null
                        : process_expression(source.patterns[5].patterns[0], context)
                    );
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        Expression process_expression(Pattern_Source source, Context context)
        {
            if (source.patterns.Length == 1)
                return process_expression_part(source.patterns[0], context);

            if (source.patterns.Length == 2)
                return new Operation(source.text, source.patterns.Select(p => process_expression_part(p, context)));

            throw new Exception("Not supported.");
        }

        Expression process_expression_part(Pattern_Source source, Context context)
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
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        Expression process_reference(Pattern_Source source, Context context)
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

            foreach (var pattern in patterns)
            {
                var token = pattern.text;
                Expression array_access = pattern.patterns.Length > 0
                    ? process_expression(pattern.patterns[0], context)
                    : null;

                Portal portal = null;
                Expression next = null;
                var symbol = context.scope.find_or_null(token);
                if (symbol != null)
                {
                    next = new Variable(symbol) { index = array_access };
                    var profession = next.get_profession();
                    if (profession != null)
                        dungeon = profession.dungeon;
                    else
                    {
                        var rail = next.get_signature().rail;
                        if (rail != null)
                            dungeon = overlord.get_dungeon(rail);
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
                        Function_Call func = null;
                        var imp = dungeon != null
                                      ? dungeon.summon_imp(token, true)
                                      : null;

                        if (imp != null)
                        {
                            func = new Class_Function_Call(imp, result);
                        }
                        else if (Imp.platform_specific_functions.Contains(token))
                        {
                            func = new Platform_Function(token, result, null);
                        }
                        else
                        {
                            var insert = context.get_expression_pattern(token);
                            if (insert != null)
                            {
                                next = insert;
                            }
                            else
                            {
                                throw new Exception("Invalid path token: " + token);
                            }
                        }

                        if (func != null)
                        {
                            if (source.patterns[1].patterns.Length > 0)
                            {
                                func.args =
                                    source.patterns[1].patterns[0].patterns.Select(p => process_expression(p, context))
                                                                  .ToArray();
                            }

                            return func;
                        }
                    }
                }

                if (result == null)
                    result = next;
                else
                    last.child = next;

                last = next;
            }

            return result;
        }

        Parameter process_parameter(Pattern_Source source, Context context)
        {
            return new Parameter(new Symbol(source.patterns[1].text, new Signature(Kind.unknown), null));
        }

        Signature parse_type(Pattern_Source source, Context context)
        {
            source = source.patterns[2];
            var text = source.patterns.Last().text;

            if (source.patterns.Length == 1)
            {
                switch (text)
                {
                    case "bool":
                        return new Signature(Kind.Bool);
                    case "string":
                        return new Signature(Kind.String);
                    case "float":
                        return new Signature(Kind.Float);
                    case "int":
                        return new Signature(Kind.Int);
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
                return new Signature(Kind.reference, realm.dungeons[text].rail);

            var dungeon = overlord.get_dungeon(text);
            if (dungeon != null)
                return new Signature(Kind.reference, dungeon.rail);

            throw new Exception("Invalid type: " + text + ".");
        }

        Profession parse_type2(Pattern_Source source, Context context)
        {
            var path = source.patterns[2].patterns;
            var text = path.Last().text;
            var is_list = source.patterns.Length > 1 && source.patterns[3].patterns.Length > 0;

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
                    realm = overlord.realms[path[i].text];
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

        Expression process_assignment(Pattern_Source source, Context context)
        {
            var reference = process_reference(source.patterns[0], context);
            var expression = process_expression(source.patterns[4], context);
            var op = source.patterns[2].text;
            var last = Expression.get_end(reference);
            //if (last.type == Expression_Type.property)
            //{
            //    var tie_expression = (Tie_Expression)last;
            //    if (tie_expression.tie.type != Kind.list && op != "=")
            //    {
            //        expression = Imp.operation(op[0].ToString(), reference.clone(), expression);
            //    }

            //    if (last != reference)
            //    {
            //        last.parent.child = null;
            //        last.parent = null;
            //    }

            //    return new Property_Function_Call(Property_Function_Type.set, tie_expression.tie, new List<Expression>
            //        {
            //            expression
            //        }) { reference = reference };
            //}

            if (last.type == Expression_Type.portal)
            {
                var portal_expression = (Portal_Expression)last;
                var portal = portal_expression.portal;
                if (portal.type != Kind.list && op != "=")
                {
                    expression = Imp.operation(op[0].ToString(), reference.clone(), expression);
                }

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

                return new Property_Function_Call(Property_Function_Type.set, portal, new List<Expression>
                    {
                        expression
                    }) { reference = reference };
            }

            return new Assignment(
                        reference,
                        op,
                        expression
                    );
        }

        public Expression process_iterator(Pattern_Source source, Context context)
        {
            var reference = process_expression_part(source.patterns[10], context);
            var profession = reference.get_profession().get_reference();
            return new Iterator(context.scope.create_symbol(source.patterns[6].text, profession),
                reference, process_block(source.patterns[14], context)
                );
        }
    }
}
