using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern;
using metahub.jackolantern.expressions;
using metahub.logic.schema;
using parser;
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
    }
}
