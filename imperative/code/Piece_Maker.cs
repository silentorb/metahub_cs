﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using metahub.Properties;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.expressions;
using metahub.logic.schema;
using metahub.parser;
using metahub.schema;

namespace metahub.imperative.code
{
    static class Piece_Maker
    {
        public static Dictionary<string, Snippet> templates = null;

        public static void initialize(Overlord overlord)
        {
            templates = overlord.summon_snippets(Resources.piecemaker_snippets);
        }

        public static void add_functions(Overlord overlord, Region region)
        {
            overlord.summon(Resources.piecemaker_imp);
            //conflict_functions(overlord, region);
            //distance_functions(overlord, region);
            //piece_maker_functions(overlord, region);
            add_groups(overlord, region);
        }

        private static void add_groups(Overlord overlord, Region region)
        {
            var dungeon = overlord.get_dungeon(region.rails["Piece_Maker"]);
            var group_dungeon = overlord.get_dungeon(region.rails["Conflict_Group"]);

            foreach (var pair in overlord.logician.groups)
            {
                var portal = dungeon.add_portal(new Portal(pair.Key + "_group",
                    Kind.reference, dungeon, group_dungeon));
                dungeon.get_block("initialize").add(new Assignment(
                    new Portal_Expression(portal), "=", new Instantiate(group_dungeon)));
            }
        }

        /*
        static void conflict_functions(Overlord overlord, Region region)
        {
            var dungeon = overlord.get_dungeon(region.rails["Conflict"]);
            var is_resolved = dungeon.spawn_imp("is_resolved");
            is_resolved.return_type = new Signature(Kind.Bool);
            is_resolved.is_abstract = true;
        }

        static void distance_functions(Overlord overlord, Region region)
        {
            var conflict = overlord.get_dungeon(region.rails["Conflict"]);
            var dungeon = overlord.get_dungeon(region.rails["Distance_Conflict"]);
            var parent = conflict.summon_imp("is_resolved");
            var is_resolved = parent.spawn_child(dungeon);
            is_resolved.expressions.Add(new Statement("return", Imp.False()));
        }
       
        static void piece_maker_functions(Overlord overlord, Region region)
        {
            var dungeon = overlord.get_dungeon(region.rails["Piece_Maker"]);
            var conflicts = dungeon.rail.get_tie_or_error("conflicts");
            var update_function = dungeon.add_function("update", new List<Parameter>());
            var if_scope = new Scope(update_function.scope);
            var conflict = if_scope.create_symbol("conflict", new Signature(Kind.reference, conflicts.other_rail));

            update_function.imp.expressions = new List<Expression>
             {
                 Imp.If(
                 Imp.operation(">", new Tie_Expression(conflicts, new Function_Call("count", null, null, true)), 
                 new Literal(0)), new List<Expression>
                 {
                     new Declare_Variable(conflict, new Tie_Expression(conflicts, 
                         new Function_Call("last", null, null, true))),

                 new Tie_Expression(conflicts, new Function_Call("pop", null, null, true))

                 })
             };
        }
        */
    }
}
