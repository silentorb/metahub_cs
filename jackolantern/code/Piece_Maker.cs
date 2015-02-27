using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern;
using metahub.jackolantern.expressions;
using metahub.jackolantern.schema;
using metahub.logic.schema;
using parser;
using metahub.schema;

namespace imperative.code
{
    static class Piece_Maker
    {
        public static Dictionary<string, Snippet> templates = null;

        public static void initialize(Overlord overlord)
        {
            templates = overlord.summon_snippets(Resources.piecemaker_snippets);
        }

        public static void add_functions(JackOLantern jack, Region region)
        {
            jack.overlord.summon(Resources.piecemaker_imp);
            //conflict_functions(overlord, region);
            //distance_functions(overlord, region);
            //piece_maker_functions(overlord, region);
            add_groups(jack, region);
        }

        private static void add_groups(JackOLantern jack, Region region)
        {
            var dungeon = jack.get_dungeon(region.rails["Piece_Maker"]);
            var group_dungeon = jack.get_dungeon(region.rails["Conflict_Group"]);

            foreach (var pair in jack.logician.groups)
            {
                var portal = dungeon.add_portal(new Portal(pair.Key + "_group",
                    Kind.reference, dungeon, group_dungeon));
                dungeon.get_block("initialize").add(new Assignment(
                    new Portal_Expression(portal), "=", new Instantiate(group_dungeon)));
            }
        }

        public static Dungeon create_conflict_class(Dungeon dungeon, JackOLantern jack)
        {
            var context = new Summoner.Context(dungeon);
            context.set_pattern("Node_Type", new Profession(Kind.reference, dungeon));
            context.set_pattern("Class_Name", "Distance_Conflict");

            var result = jack.summon_dungeon(Piece_Maker.templates["Distance_Conflict"], context);
            var portal = result.all_portals["nodes"];
            var scope = new Scope();
            scope.add_map("a", c => new Portal_Expression(portal) { index = new Literal((int)0) });
            scope.add_map("b", c => new Portal_Expression(portal) { index = new Literal((int)1) });
            var imp = result.summon_minion("is_resolved");
            var swamp = new Swamp(jack, null, context);
            imp.add_to_block(new Statement("return", new Literal(true)));

            //imp.expressions.Add(new Statement("return",
            //    new Operation(constraint.op, new List<Expression>{ 
            //        new Platform_Function("dist", new Portal_Expression(portal, 
            //            new Portal_Expression(jack.get_portal(constraint.endpoints.Last())))
            //            { index = new Literal((int)0) },
            //            new List<Expression> { new Portal_Expression(portal, new Portal_Expression(jack.get_portal(constraint.endpoints.Last())
            //                )) { index = new Literal((int)1) } }),
            //        swamp.translate_exclusive(constraint.second, null)
            //    })
            //));
            return result;
            //var base_class = overlord.realms["piecemaker"[.dungeons["Conflict"];
            //var result = new Dungeon("Distance_Conflict", overlord, dungeon.realm, base_class);
            //var portal = result.add_portal(new Portal("nodes", Kind.list, result, dungeon));
            //result.generate_code1();

            //var scope = new Scope();


            //return result;
        }
    }
}
