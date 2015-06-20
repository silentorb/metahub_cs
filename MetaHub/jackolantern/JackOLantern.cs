using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using metahub.Properties;
using imperative;
using imperative.code;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using imperative.render.artisan;
using imperative.render.targets;
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
        //public Dictionary<Trellis, Dungeon> rail_map1 = new Dictionary<Trellis, Dungeon>();
        //public Dictionary<Dungeon, Trellis> rail_map2 = new Dictionary<Dungeon, Trellis>();

        //public Dictionary<Dwarf_Clan, Dungeon> dungeons = new Dictionary<Dwarf_Clan, Dungeon>();
        public Dictionary<Dungeon, Dwarf_Clan> clans = new Dictionary<Dungeon, Dwarf_Clan>();
        public Dictionary<Trellis, Dwarf_Clan> rail_clans = new Dictionary<Trellis, Dwarf_Clan>();

        public Schema schema;
        public Common_Target2 target;

        public JackOLantern(Logician logician, Overlord overlord, Common_Target2 target)
        {
            this.logician = logician;
            this.overlord = overlord;
            this.schema = logician.schema;
            this.target = target;

            initialize_functions();

            if (Piece_Maker.templates == null)
                Piece_Maker.initialize(overlord);

            templates = overlord.summon_snippets(Resources.jackolantern_snippets, "");
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
            //            load_schema_from_vineyard();
            generate_code(target);

            foreach (var pumpkin in logician.functions)
            {
                carve_pumpkin(pumpkin);
            }

//            var not_external = overlord.dungeons.Where(d => !d.is_external).ToArray();
//            foreach (var dungeon in not_external)
//            {
//                target.generate_code2(dungeon);
//            }
        }

        public void carve_pumpkin(Function_Node pumpkin)
        {
            if (pumpkin.name[0] == '@')
                pumpkin.name = pumpkin.name.Substring(1);

            if (!carvers.ContainsKey(pumpkin.name))
                return;
//                throw new Exception("Invalid operation: " + pumpkin.name);

            var carver = carvers[pumpkin.name];
            carver.carve(pumpkin);

            // The main purpose of tracking scoping of functions is 
            // to enforce a parent -> child carving order
            foreach (var child in pumpkin.children)
            {
                carve_pumpkin(child);
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

                foreach (var tie in rail.all_properties.Values)
                {
                    Portal portal = create_portal_from_property(tie, dungeon);
                    if (rail.core_properties.ContainsKey(tie.name))
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
            var dungeon = signature.trellis != null
                ? get_dungeon(signature.trellis)
                : null;

            return Profession.create(dungeon);
        }

        public static Profession kind_to_profession(Kind kind)
        {
            switch (kind)
            {
                case Kind.Bool:
                    return Professions.Bool;
                    break;

                case Kind.String:
                    return Professions.String;
                    break;

                case Kind.Float:
                    return Professions.Float;

                case Kind.Int:
                    return Professions.Int;

            }

            throw new Exception("profession_from_kind cannot convert kind: " + kind);
        }

        public Dwarf get_dwarf(Minion minion)
        {
            var clan = clans[minion.dungeon];
            return clan.dwarves[minion];
        }

        public Dungeon create_dungeon_from_rail(Trellis rail, Dungeon realm)
        {
            var dungeon = new Dungeon(rail.name, overlord, realm, null, rail.is_value);
            dungeon.is_abstract = rail.is_abstract;
            dungeon.default_value = rail.default_value;

            if (realm.trellis_additional.ContainsKey(rail.name))
            {
                var map = realm.trellis_additional[rail.name];

                if (map.inserts != null)
                    dungeon.inserts = map.inserts;
            }

            return dungeon;
        }

        public Trellis get_rail(Dungeon dungeon)
        {
            return clans[dungeon].trellis;
        }

        public Dwarf_Clan add_clan(Dungeon dungeon, Trellis rail = null)
        {
            var clan = new Dwarf_Clan(this, dungeon, rail);
            clans[dungeon] = clan;
            if (rail != null)
                rail_clans[rail] = clan;

            return clan;
        }

        private void load_schema(Schema vineyard_schema)
        {
            var realm = overlord.root.create_dungeon(vineyard_schema.name);

            foreach (var rail in vineyard_schema.trellises.Values)
            {
                Dungeon dungeon = create_dungeon_from_rail(rail, realm);
                realm.dungeons[dungeon.name] = dungeon;
                add_clan(dungeon, rail);
            }
            foreach (var child in vineyard_schema.children.Values)
            {
                load_schema(child);
            }
        }

        public void load_schema_from_vineyard()
        {
            load_schema(schema);

            foreach (var dungeon in overlord.dungeons)
            {
                initialize_dungeon(dungeon);
            }

        }

        public void generate_code(Common_Target2 target)
        {
            if (target.GetType() == typeof(Csharp))
                overlord.summon(Utility.load_resource("metahub_cs.imp"), "internal.metahub_cs.imp");
            else
                overlord.summon(Resources.metahub_imp, "internal.metahub.imp");

//            overlord.summon(Resources.piecemaker_imp, "piecemaker");

//            if (logician.schema.children.ContainsKey("piecemaker"))
//            {
//                var piece_region = logician.schema.children["piecemaker"];
//                Piece_Maker.add_functions(this, piece_region);
//            }
            var not_external = overlord.dungeons.Where(d => !d.is_external).ToArray();
            foreach (var dungeon in not_external)
            {
//                dungeon.generate_code();
                if (clans.ContainsKey(dungeon))
                    clans[dungeon].generate_code1();
            }

            foreach (var dungeon in not_external)
            {
//                target.generate_dungeon_code(dungeon);
                if (clans.ContainsKey(dungeon))
                    clans[dungeon].generate_code2();
            }

        }

        public Property get_tie(Portal portal)
        {
            var rail = get_rail(portal.dungeon);
            return rail.all_properties.ContainsKey(portal.name)
                ? rail.all_properties[portal.name]
                : null;
        }

        public Dungeon get_dungeon(Trellis rail)
        {
            if (!rail_clans.ContainsKey(rail))
                return null;

            return rail_clans[rail].dungeon;
        }

        public Dungeon get_dungeon_or_error(Trellis rail)
        {
            if (!rail_clans.ContainsKey(rail))
                throw new Exception("Could not find dungeon for rail " + rail.name + ".");

            return rail_clans[rail].dungeon;
        }

        public Portal get_portal(Property tie)
        {
            var dungeon = get_dungeon(tie.trellis);
            if (!dungeon.has_portal(tie.name))
                return null;

            return dungeon.all_portals[tie.name];
        }

        public Dungeon summon_dungeon(Snippet template, Summoner_Context context)
        {
            var dungeon = overlord.summon_dungeon(template, context);
            var clan = add_clan(dungeon);
//            target.generate_dungeon_code(dungeon);
            return dungeon;
        }

        public Portal create_portal_from_property(Property tie, Dungeon dungeon)
        {
            var other_dungeon = tie.other_trellis != null
                    ? get_dungeon(tie.other_trellis)
                    : null;
            var portal = new Portal(tie.name, tie.type, dungeon, other_dungeon);

            if (tie.other_property != null)
            {
                var d = get_dungeon(tie.other_trellis);
                if (d != null)
                {
                    portal.other_portal = get_portal(tie.other_property);
                    if (portal.other_portal != null)
                    {
                        portal.other_portal.other_portal = portal;
                    }
                }
            }

            portal.is_value = tie.is_value;
//            portal.default_expression = new Literal(tie.default_value);

            return portal;
        }

        public Minion get_setter(Portal portal)
        {
            return portal.setter ?? clans[portal.dungeon].generate_setter(portal);
        }

        public Expression summon_snippet(string name, Summoner_Context context)
        {
            return overlord.summon_snippet(templates[name], context);
        }

        public List<Expression> summon_snippet_block(string name, Summoner_Context context)
        {
            var statements = (Block)overlord.summon_snippet(templates[name], context);
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
