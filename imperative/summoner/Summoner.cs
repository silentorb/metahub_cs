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
        public Definition parser_definition;
        private Overlord overlord;

        public Summoner(Overlord overlord)
        {
            this.overlord = overlord;
        }

        public void summon(string code)
        {
            if (parser_definition == null)
            {
                load_parser();
            }
            var context = new Pre_Summoner(parser_definition);
            var without_comments = Hub.remove_comments.Replace(code, "");
            //trace("without_comments", without_comments);
            var result = context.parse(without_comments);

            //Debug_Info.output(result);
            if (!result.success)
                throw new Exception("Syntax Error at " + result.end.y + ":" + result.end.x);

            var match = (Match)result;
            process_root(match.get_data());
        }

        public void load_parser()
        {
            Definition boot_definition = new Definition();
            boot_definition.load_parser_schema();
            Bootstrap context = new Bootstrap(boot_definition);

            var result = context.parse(Resources.imp_grammar, false);
            //Debug_Info.output(result);
            if (result.success)
            {
                var match = (Match)result;

                parser_definition = new Definition();
                parser_definition.load(match.get_data().dictionary);
            }
            else
            {
                throw new Exception("Error loading parser.");
            }
        }

        void process_root(Pattern_Source source)
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
            var dungeon = context.realm.dungeons[name];
            var dungeon_context = new Context(context.realm, dungeon);
            foreach (var statement in statements)
            {
                process_function_definitions(statement, dungeon_context);
            }
        }

        void process_function_definitions(Pattern_Source source, Context context)
        {
            var imp = context.dungeon.spawn_imp(
                source.patterns[1].text,
                source.patterns[4].patterns.Select(p => process_parameter(p, context)).ToList()
            );

            var attributes = source.patterns[0];
            if (attributes.patterns.Length > 0)
                imp.is_abstract = attributes.patterns[0].patterns[0].text == "abstract";

            var return_type = source.patterns[7];
            if (return_type.patterns.Length > 0)
                imp.return_type = parse_type(return_type.patterns[0].patterns[2].text);

            imp.expressions = process_block(source.patterns[9], context);
        }

        List<Expression> process_block(Pattern_Source source, Context context)
        {
            return source.patterns[2].patterns.Select(p => process_statement(p, context)).ToList();
        }

        Expression process_statement(Pattern_Source source, Context context)
        {
            switch (source.type)
            {
                case "if":
                    return new Flow_Control(Flow_Control_Type.If, process_expressions(source.patterns[4], context),
                        process_block(source.patterns[8], context)
                    );

                case "return":
                    return new Statement("return", source.patterns[1].patterns.Length == 0
                        ? null
                        : process_expressions(source.patterns[1].patterns[0], context)
                    );
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        Expression process_expressions(Pattern_Source source, Context context)
        {
            if (source.patterns.Length == 1)
            return process_expression(source.patterns[0], context);
            
            return new Operation(source.dividers[0], source.patterns.Select(p=>process_expression(p, context)));
        }

        Expression process_expression(Pattern_Source source, Context context)
        {
            switch (source.type)
            {
                case "bool":
                    return new Literal(source.text == "true", new Signature(Kind.Bool));

                case "int":
                    return new Literal(int.Parse(source.text), new Signature(Kind.Int));

                case "reference":
                    return process_reference(source, context);
            }

            throw new Exception("Unsupported statement type: " + source.type + ".");
        }

        Expression process_reference(Pattern_Source source, Context context)
        {
            var dungeon = context.dungeon;
            Expression result = null;
            Expression last = null;
            foreach (var pattern in source.patterns)
            {
                var token = pattern.text;
                Tie tie = null;
                if (dungeon != null)
                    tie = dungeon.rail.get_tie_or_null(token);

                Expression next;
                if (tie != null)
                    next = new Tie_Expression(tie);
                else
                {
                    var imp = context.dungeon.summon_imp(token);
                    next = imp != null 
                        ? new Function_Call(imp) 
                        : new Function_Call(token, null, true);
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

        Signature parse_type(string text)
        {
            switch (text)
            {
                case "bool": return new Signature(Kind.Bool);
                case "string": return new Signature(Kind.String);
                case "float": return new Signature(Kind.Float);
                case "int": return new Signature(Kind.Int);
            }

            throw new Exception("Invalid type: " + text + ".");
        }
    }
}
