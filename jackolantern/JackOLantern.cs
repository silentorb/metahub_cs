using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using imperative;
using imperative.code;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern.carvers;
using metahub.jackolantern.code;
using metahub.jackolantern.expressions;
using metahub.logic;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;
using Block = imperative.expressions.Block;

namespace metahub.jackolantern
{
    public class JackOLantern
    {
        public Logician logician;
        public Overlord overlord;
        public Dictionary<string, Carver> carvers = new Dictionary<string, Carver>();
        public Dictionary<string, Snippet> templates = null;
        //public Dictionary<Rail, Dungeon> rail_map1 = new Dictionary<Rail, Dungeon>();
        //public Dictionary<Dungeon, Rail> rail_map2 = new Dictionary<Dungeon, Rail>();

        //public Dictionary<Dwarf_Clan, Dungeon> dungeons = new Dictionary<Dwarf_Clan, Dungeon>();
        public Dictionary<Dungeon, Dwarf_Clan> clans = new Dictionary<Dungeon, Dwarf_Clan>();
        public Dictionary<Rail, Dwarf_Clan> rail_clans = new Dictionary<Rail, Dwarf_Clan>();

        public Railway railway;
        public Target target;

        public JackOLantern(Logician logician, Overlord overlord, Railway railway, Target target)
        {
            this.logician = logician;
            this.overlord = overlord;
            this.railway = railway;
            this.target = target;

            initialize_functions();

            if (Piece_Maker.templates == null)
                Piece_Maker.initialize(overlord);

            templates = overlord.summon_snippets(Resources.jackolantern_snippets);
        }

        public void initialize_functions()
        {
            carvers["="] = new Equals(this);
            carvers["contains"] = new Contains(this);
            carvers["count"] = new Count(this);
            carvers["cross"] = new Cross(this);
            carvers["in"] = new In(this);
            carvers["map"] = new Map(this);
        }

        public void run()
        {
            generate_code(target);

            foreach (var pumpkin in logician.functions)
            {
                carve_pumpkin(pumpkin);
            }

            var not_external = overlord.dungeons.Where(d => !d.is_external).ToArray();
            foreach (var dungeon in not_external)
            {
                target.generate_code2(dungeon);
            }

            overlord.flatten();
            overlord.post_analyze();
        }

        public void carve_pumpkin(Function_Node pumpkin)
        {
            if (pumpkin.name[0] == '@')
                pumpkin.name = pumpkin.name.Substring(1);

            if (carvers.ContainsKey(pumpkin.name))
            {
                var carver = carvers[pumpkin.name];
                carver.carve(pumpkin);

                // The main purpose of tracking scoping of functions is 
                // to enforce a parent -> child carving order
                foreach (var child in pumpkin.children)
                {
                    carve_pumpkin(child);
                }
            }
        }

        public static string get_setter_name(Portal portal)
        {
            return portal.is_list
                ? "add_" + portal.name
                : "set_" + portal.name;
        }

        public Minion get_initialize(Dungeon dungeon)
        {
            return dungeon.summon_minion("initialize");
        }

        public void initialize_dungeon(Dungeon dungeon)
        {
            var rail = get_rail(dungeon);
            if (rail != null)
            {
                if (rail.parent != null)
                    dungeon.parent = get_dungeon(rail.parent);

                foreach (var tie in rail.all_ties.Values)
                {
                    Portal portal = create_portal_from_tie(tie, dungeon);
                    if (rail.core_ties.ContainsKey(tie.name))
                        dungeon.add_portal(portal);
                    else
                        dungeon.add_parent_portal(portal);

                    //if (rail.core_ties.ContainsKey(tie.name))
                    //    dungeon.core_portals[tie.name] = portal;
                }
            }
        }

        public Profession get_profession(Signature signature)
        {
            var dungeon = signature.rail != null
                ? get_dungeon(signature.rail)
                : null;

            return new Profession(signature.type, dungeon);
        }

        public Dwarf get_dwarf(Minion minion)
        {
            var clan = clans[minion.dungeon];
            return clan.dwarves[minion];
        }

        public Dungeon create_dungeon_from_rail(Rail rail, Realm realm)
        {
            var dungeon = new Dungeon(rail.name, overlord, realm);
            dungeon.is_abstract = rail.trellis.is_abstract;
            dungeon.is_value = rail.trellis.is_value;
            dungeon.default_value = rail.default_value;

            if (realm.trellis_additional.ContainsKey(rail.trellis.name))
            {
                var map = realm.trellis_additional[rail.trellis.name];

                if (map.inserts != null)
                    dungeon.inserts = map.inserts;
            }

            return dungeon;
        }

        public Rail get_rail(Dungeon dungeon)
        {
            return clans[dungeon].rail;
        }

        public Dwarf_Clan add_clan(Dungeon dungeon, Rail rail = null)
        {
            var clan = new Dwarf_Clan(this, dungeon, rail);
            clans[dungeon] = clan;
            if (rail != null)
                rail_clans[rail] = clan;

            return clan;
        }

        public void generate_code(Target target)
        {
            foreach (var region in railway.regions.Values)
            {
                //if (region.is_external)
                //    continue;

                var realm = new Realm(region.name, overlord);
                if (region.space.additional != null && region.space.additional.ContainsKey(region.name))
                    realm.load_additional(region.space.additional[region.name]);
                realm.external_name = region.external_name;
                overlord.realms[realm.name] = realm;

                foreach (var rail in region.rails.Values)
                {

                    Dungeon dungeon = create_dungeon_from_rail(rail, realm);
                    realm.dungeons[dungeon.name] = dungeon;
                    add_clan(dungeon, rail);

                    //if (rail.trellis.is_abstract && rail.trellis.is_value)
                    //    continue;

                    //overlord.dungeons.Add(dungeon);
                }
            }

            foreach (var dungeon in overlord.dungeons)
            {
                initialize_dungeon(dungeon);
            }

            foreach (var dungeon in overlord.dungeons.Where(d => !d.is_external))
            {
                var rail = get_rail(dungeon);
                rail.finalize();
            }

            var not_external = overlord.dungeons.Where(d => !d.is_external).ToArray();

            foreach (var dungeon in not_external)
            {
                dungeon.generate_code();
                clans[dungeon].generate_code1();
            }

            foreach (var dungeon in not_external)
            {
                target.generate_dungeon_code(dungeon);
                clans[dungeon].generate_code2();
            }

            overlord.summon(Resources.metahub_imp);

            if (logician.railway.regions.ContainsKey("piecemaker"))
            {
                var piece_region = logician.railway.regions["piecemaker"];
                Piece_Maker.add_functions(this, piece_region);
            }
        }

        public Tie get_tie(Portal portal)
        {
            var rail = get_rail(portal.dungeon);
            return rail.all_ties.ContainsKey(portal.name)
                ? rail.all_ties[portal.name]
                : null;
        }

        public Rail get_rail(Trellis trellis)
        {
            return railway.get_rail(trellis);
        }

        public Dungeon get_dungeon(Rail rail)
        {
            if (!rail_clans.ContainsKey(rail))
                return null;

            return rail_clans[rail].dungeon;
        }

        public Dungeon get_dungeon_or_error(Rail rail)
        {
            if (!rail_clans.ContainsKey(rail))
                throw new Exception("Could not find dungeon for rail " + rail.name + ".");

            return rail_clans[rail].dungeon;
        }

        public Portal get_portal(Tie tie)
        {
            var dungeon = get_dungeon(tie.rail);
            if (!dungeon.has_portal(tie.tie_name))
                return null;

            return dungeon.all_portals[tie.tie_name];
        }

        public Dungeon summon_dungeon(Snippet template, Summoner.Context context)
        {
            var dungeon = overlord.summon_dungeon(template, context);
            var clan = add_clan(dungeon);
            target.generate_dungeon_code(dungeon);
            return dungeon;
        }

        public Portal create_portal_from_tie(Tie tie, Dungeon dungeon)
        {
            var other_dungeon = tie.other_rail != null
                    ? get_dungeon(tie.other_rail)
                    : null;
            var portal = new Portal(tie.name, tie.type, dungeon, other_dungeon);

            if (tie.other_tie != null)
            {
                var d = get_dungeon(tie.other_rail);
                if (d != null)
                {
                    portal.other_portal = get_portal(tie.other_tie);
                    if (portal.other_portal != null)
                    {
                        portal.other_portal.other_portal = portal;
                    }
                }
            }

            portal.is_value = tie.is_value;
            if (tie.property != null)
                portal.default_value = tie.property.default_value;

            return portal;
        }

        public Minion get_setter(Portal portal)
        {
            return portal.setter ?? clans[portal.dungeon].generate_setter(portal);
        }

        public Expression summon_snippet(string name, Summoner.Context context)
        {
            return overlord.summon_snippet(templates[name], context);
        }

        public List<Expression> summon_snippet_block(string name, Summoner.Context context)
        {
            var statements = (Block) overlord.summon_snippet(templates[name], context);
            return statements.body;
        }

        public Property_Function_Call call_setter(Portal portal, Expression reference, Expression value, Expression origin)
        {
            var minion = get_setter(portal);
            return new Property_Function_Call(Property_Function_Type.set, portal,
                origin != null && minion.parameters.Count > 1
                ? new List<Expression> { value, origin }
                : new List<Expression> { value }
             ) { reference = reference };
        }

    }
}
