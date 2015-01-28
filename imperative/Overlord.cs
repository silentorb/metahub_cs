using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using metahub.Properties;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Literal = metahub.imperative.types.Literal;
using Logician = metahub.logic.Logician;
using Node_Type = metahub.logic.nodes.Node_Type;
using Node = metahub.logic.nodes.Node;

namespace metahub.imperative
{
    public class Overlord
    {
        public Railway railway;
        public List<Dungeon> dungeons = new List<Dungeon>();
        public Dictionary<Rail, Dungeon> rail_map = new Dictionary<Rail, Dungeon>();
        public Dictionary<string, Realm> realms = new Dictionary<string, Realm>();
        public Target target;
        public Logician logician;

        public Overlord(Hub hub, string target_name)
        {
            if (Platform_Function_Info.functions == null)
                Platform_Function_Info.initialize();

            railway = new Railway(hub, target_name);
            if (Piece_Maker.templates == null)
                Piece_Maker.initialize(this);
        }

        public void run(Logician logician, Target target)
        {
            this.logician = logician;
            this.target = target;

        }

        public void finalize()
        {
            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.rail.finalize();
            }
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

        public Dungeon get_dungeon(Rail rail)
        {
            if (!rail_map.ContainsKey(rail))
                return null;

            return rail_map[rail];
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

        public Dungeon get_dungeon_or_error(Rail rail)
        {
            if (!rail_map.ContainsKey(rail))
                throw new Exception("Could not find dungeon for rail " + rail.name + ".");

            return rail_map[rail];
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

        public Rail get_rail(Trellis trellis)
        {
            return railway.get_rail(trellis);
        }

        public Portal get_portal(Tie tie)
        {
            var dungeon = get_dungeon(tie.rail);
            return dungeon.all_portals[tie.tie_name];
        }

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

        public Dungeon summon_dungeon(Template template, Summoner.Context context)
        {
            var summoner = new Summoner(this);
            summoner.process_dungeon1(template.source, context);
            return summoner.process_dungeon2(template.source, context);
        }

        public Expression summon_snippet(Template template, Summoner.Context context)
        {
            var summoner = new Summoner(this);
            return summoner.process_statement(template.source, context);
        }

        public Dictionary<string, Template> summon_snippets(string code)
        {
            var templates = new Dictionary<string, Template>();
            var match = Regex.Matches(code,
                @"@@@[ \t]*(\w+)[ \t]*\([ \t]*(.*?)[ \t]*\)[ \t]*\r\n(.*?)(?=@@@|\z)", RegexOptions.Singleline);
            foreach (Match item in match)
            {
                foreach (Match capture in item.Captures)
                {
                    var name = capture.Groups[1].Value;
                    var parameters = Regex.Split(capture.Groups[2].Value, @"\s*,\s*");
                    var block = capture.Groups[3].Value;
                    var pre_summoner = pre_summon(block, Pre_Summoner.Mode.snippet);
                    templates[name] = new Template(name, parameters, pre_summoner.output.patterns[1]);
                }
            }

            return templates;
        }
    }
}