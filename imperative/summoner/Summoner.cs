using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.lab;
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
            var realm = overlord.realms[name];
            var context = new Context(realm);
            var statements = source.patterns[6].patterns;
            foreach (var statement in statements)
            {
                process_class(statement, context);
            }
        }

        void process_class(Pattern_Source source, Context context)
        {
            var name = source.patterns[2].text;
            var statements = source.patterns[6].patterns;
            Dungeon dungeon = null;

            if (context.realm.dungeons.ContainsKey(name))
            {
                dungeon = context.realm.dungeons[name];
            }
            else
            {
                dungeon = context.realm.create_dungeon(name);
            }

            var dungeon_context = new Context(context.realm, dungeon);
            foreach (var statement in statements)
            {
                process_function_definition(statement, dungeon_context);
            }
        }

        void process_function_definition(Pattern_Source source, Context context)
        {
            if (source.type == "abstract_function")
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
            else
            {
                var imp = context.dungeon.spawn_imp(
                    source.patterns[1].text,
                    source.patterns[4].patterns.Select(p => process_parameter(p, context)).ToList()
                );
                var new_context = new Context(context) { scope = imp.scope };

                //var attributes = source.patterns[0];
                //if (source.type == "abstract_function")
                //    imp.is_abstract = true;

                var return_type = source.patterns[7];
                if (return_type.patterns.Length > 0)
                    imp.return_type = parse_type(return_type.patterns[0], context);

                imp.expressions = process_block(source.patterns[9], new_context);
            }

        }

        List<Expression> process_block(Pattern_Source source, Context context)
        {
            return source.patterns.Select(p => process_statement(p, context)).ToList();
        }

        Expression process_statement(Pattern_Source source, Context context)
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
            foreach (var pattern in source.patterns[0].patterns)
            {
                var token = pattern.text;
                Expression array_access = pattern.patterns.Length > 0
                    ? process_expression(pattern.patterns[0], context)
                    : null;

                Tie tie = null;
                Expression next;
                var symbol = context.scope.find_or_null(token);
                if (symbol != null)
                {
                    next = new Variable(symbol) { index = array_access };

                    var rail = next.get_signature().rail;
                    if (rail != null)
                        dungeon = overlord.get_dungeon(rail);
                }
                else
                {
                    if (dungeon != null)
                        tie = dungeon.rail.get_tie_or_null(token);

                    if (tie != null)
                    {
                        next = new Tie_Expression(tie) { index = array_access };
                        dungeon = tie.other_rail != null
                            ? overlord.get_dungeon(tie.other_rail)
                            : null;
                    }
                    else
                    {
                        var imp = dungeon != null
                            ? dungeon.summon_imp(token)
                            : null;

                        var func = imp != null
                            ? new Function_Call(imp)
                            : new Function_Call(token, null, null, true);

                        func.reference = result;
                        if (source.patterns[1].patterns.Length > 0)
                            func.args = source.patterns[1].patterns[0].patterns.Select(p => process_expression(p, context)).ToArray();

                        return func;
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

            throw new Exception("Invalid type: " + text + ".");
        }

        Expression process_assignment(Pattern_Source source, Context context)
        {
            var reference = process_reference(source.patterns[0], context);
            var expression = process_expression(source.patterns[4], context);
            var op = source.patterns[2].text;
            var last = Expression.get_end(reference);
            if (last.type == Expression_Type.property)
            {
                var tie_expression = (Tie_Expression)last;
                if (op != "=")
                {
                    expression = Imp.operation(op[0].ToString(), reference.clone(), expression);
                }

                if (last != reference)
                {
                    last.parent.child = null;
                    last.parent = null;
                }

                return new Property_Function_Call(Property_Function_Type.set, tie_expression.tie, new List<Expression>
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
    }
}
