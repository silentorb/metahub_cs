using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern;
using metahub.jackolantern.expressions;

using metahub.render;
using metahub.schema;
using Literal = metahub.imperative.types.Literal;

namespace metahub.imperative
{
    public class Overlord
    {
        public List<Dungeon> dungeons = new List<Dungeon>();
        public Dictionary<string, Realm> realms = new Dictionary<string, Realm>();
        public Target target;

        public Overlord()
        {
            if (Platform_Function_Info.functions == null)
                Platform_Function_Info.initialize();

        }

        public void run(Target target)
        {
            this.target = target;

        }

        public void post_analyze()
        {
            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.analyze();
            }
        }

        public void flatten()
        {
            var temp = dungeons.Where(d => !d.is_external).Select(d=>d.name);

            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.flatten();
            }
        }

        public Dungeon get_dungeon(string name)
        {
            foreach (var realm in realms.Values)
            {
                if (realm.dungeons.ContainsKey(name))
                    return realm.dungeons[name];
            }
            
            return null;
        } 

        //Node create_lambda_constraint (metahub Node.meta.types.Constraint, Scope scope) {
        //throw "";
        //var rail = get_rail(scope.trellis);
        //metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(Node, this);
        //var tie = Parse.get_end_tie(constraint.reference);
        //trace("tie", tie.rail.name + "." + tie.name);
        //tie.constraints.Add(constraint);
        //constraints.Add(constraint);
        //return null;
        //}

        public Pre_Summoner pre_summon(string code, Pre_Summoner.Mode mode = Pre_Summoner.Mode.full)
        {
            var pre_summoner = new Pre_Summoner();
            pre_summoner.summon(code, mode);
            return pre_summoner;
        }

        public void summon(Pre_Summoner pre_summoner)
        {
            var summoner = new Summoner(this);
            summoner.summon(pre_summoner.output);
        }

        public void summon(string code)
        {
            var pre_summoner = pre_summon(code);
            summon(pre_summoner);
        }

        public Dungeon summon_dungeon(Snippet template, Summoner.Context context)
        {
            var summoner = new Summoner(this);
            summoner.process_dungeon1(template.source, context);
            return summoner.process_dungeon2(template.source, context);
        }

        public Expression summon_snippet(Snippet template, Summoner.Context context)
        {
            var summoner = new Summoner(this);
            return summoner.process_statement(template.source, context);
        }

        public Dictionary<string, Snippet> summon_snippets(string code)
        {
            var templates = new Dictionary<string, Snippet>();
            //var match = Regex.Matches(code,
            //    @"@@@[ \t]*(\w+)[ \t]*\([ \t]*(.*?)[ \t]*\)[ \t]*\r\n(.*?)(?=@@@|\z)", RegexOptions.Singleline);
            //foreach (Match item in match)
            //{
            //    foreach (Match capture in item.Captures)
            //    {
            //        var name = capture.Groups[1].Value;
            //        var parameters = Regex.Split(capture.Groups[2].Value, @"\s*,\s*");
            //        var block = capture.Groups[3].Value;
            //        var pre_summoner = pre_summon(block, Pre_Summoner.Mode.snippet);
            //        templates[name] = new Template(name, parameters, pre_summoner.output.patterns[1]);
            //    }
            //}

            var pre_summoner = pre_summon(code, Pre_Summoner.Mode.snippet);
            var summoner = new Summoner(this);

            var context = new Summoner.Context();
            var statements = (Statements)summoner.process_statement(pre_summoner.output, context);
            foreach (Snippet snippet in statements.children)
            {
                templates[snippet.name] = snippet;
            }
            return templates;
        }
    }
}