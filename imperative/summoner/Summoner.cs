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
                source.patterns[0].text,
                source.patterns[3].patterns.Select(p => process_parameter(p, context)).ToList()
            );
            
        }

        Parameter process_parameter(Pattern_Source source, Context context)
        {
            return new Parameter(new Symbol(source.patterns[1].text, new Signature(Kind.unknown), null));
        }
    }
}
