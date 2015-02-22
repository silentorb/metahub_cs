using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using metahub.imperative;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.jackolantern.carvers;
using metahub.jackolantern.code;
using metahub.jackolantern.expressions;
using metahub.logic;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;

namespace metahub.jackolantern
{
    public class JackOLantern
    {
        public Logician logician;
        public Overlord overlord;
        public Dictionary<string, Carver> carvers = new Dictionary<string, Carver>();
        public Dictionary<string, Snippet> templates = null;
        public Dictionary<Rail, Dungeon> rail_map1 = new Dictionary<Rail, Dungeon>();
        public Dictionary<Dungeon, Rail> rail_map2 = new Dictionary<Dungeon, Rail>();
        public Railway railway;

        public JackOLantern(Logician logician, Overlord overlord, Hub hub, Railway railway)
        {
            this.logician = logician;
            this.overlord = overlord;
            this.railway = railway;
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

        public void run(Target target)
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

        public Imp get_initialize(Dungeon dungeon)
        {
            return dungeon.summon_imp("initialize");
        }

        public void implement_constraint(Constraint constraint)
        {
            //var ties = Parse.get_endpoints(constraint.first);
            foreach (var tie in constraint.endpoints.Where(tie => tie != null))
            {
                if (tie.type == Kind.list)
                {
                    List_Code.generate_constraint(constraint, this);
                }
                else
                {
                    Reference.generate_constraint(constraint, tie, this);
                }
            }
        }



        public void initialize_dungeon(Dungeon dungeon)
        {
            var rail = get_rail(dungeon);

            if (rail.parent != null)
                dungeon.parent = get_dungeon(rail.parent);

            if (rail != null)
            {
                foreach (var tie in rail.all_ties.Values)
                {
                    Portal portal = create_portal_from_tie(tie, dungeon);
                    dungeon.all_portals[tie.name] = portal;
                    if (rail.core_ties.ContainsKey(tie.name))
                        dungeon.core_portals[tie.name] = portal;
                }
            }

            //foreach (var portal in dungeon.core_portals.Values)
            //{
            //    if (portal.rail != null)
            //        portal.dungeon = overlord.get_dungeon_or_error(portal.rail);

            //    if (portal.other_rail != null)
            //        portal.other_dungeon = overlord.get_dungeon_or_error(portal.other_rail);
            //}
        }

        public Profession get_profession(Signature signature)
        {
            var dungeon = signature.rail != null
                ? get_dungeon(signature.rail)
                : null;

            return new Profession(signature.type, dungeon);
        }
        public Dungeon create_dungeon_from_rail(Rail rail, Realm realm)
        {
            var dungeon = new Dungeon(rail.name, overlord, realm);
            dungeon.is_external = rail.is_external;
            dungeon.is_abstract = rail.trellis.is_abstract;
            dungeon.is_value = rail.trellis.is_value;
            dungeon.source_file = rail.source_file;
            dungeon.stubs = rail.stubs;
            dungeon.hooks = rail.hooks;
            dungeon.class_export = rail.class_export;

            var region = rail.region;
            if (region.trellis_additional.ContainsKey(rail.trellis.name))
            {
                var map = region.trellis_additional[rail.trellis.name];

                if (map.inserts != null)
                    dungeon.inserts = map.inserts;
            }

            return dungeon;
        }

        public Rail get_rail(Dungeon dungeon)
        {
            return rail_map2[dungeon];
        }

        public void generate_code(Target target)
        {
            foreach (var region in railway.regions.Values)
            {
                //if (region.is_external)
                //    continue;

                var realm = new Realm(region.name, overlord);
                overlord.realms[realm.name] = realm;

                foreach (var rail in region.rails.Values)
                {

                    Dungeon dungeon = create_dungeon_from_rail(rail, realm);
                    realm.dungeons[dungeon.name] = dungeon;
                    rail_map1[rail] = dungeon;
                    rail_map2[dungeon] = rail;

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
                Dungeon_Carver.generate_code1(this, dungeon, get_rail(dungeon));
            }

            foreach (var dungeon in not_external)
            {
                target.generate_rail_code(dungeon);
                Dungeon_Carver.generate_code2(this, dungeon, get_rail(dungeon));
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
            if (!rail_map1.ContainsKey(rail))
                return null;

                return rail_map1[rail];
        }

        public Dungeon get_dungeon_or_error(Rail rail)
        {
            if (!rail_map1.ContainsKey(rail))
                throw new Exception("Could not find dungeon for rail " + rail.name + ".");

            return rail_map1[rail];
        }

        public Portal get_portal(Tie tie)
        {
            var dungeon = get_dungeon(tie.rail);
            if (!dungeon.all_portals.ContainsKey(tie.tie_name))
                return null;

            return dungeon.all_portals[tie.tie_name];
        }

        public Portal create_portal_from_tie(Tie tie, Dungeon dungeon)
        {
            var other_dungeon = tie.other_rail != null
                    ? get_dungeon(tie.other_rail)
                    : null;
            var portal = new Portal(tie.name, tie.type, dungeon, other_dungeon);
            //profession = new Profession(tie.type);

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
            return portal;
        }

        public Imp get_setter(Portal portal)
        {
            var setter = portal.dungeon.summon_imp(get_setter_name(portal))
                ?? Dungeon_Carver.generate_setter(portal);

            return setter;
        }

        Expression package_path(IEnumerable<Expression> path)
        {
            if (path.Count() == 1)
                return path.First();

            return new Path(path);
        }

        public Property_Function_Call call_setter(Portal portal, Expression reference, Expression value, Expression origin)
        {
            var imp = get_setter(portal);
            return new Property_Function_Call(Property_Function_Type.set, portal,
                origin != null && imp.parameters.Count > 1
                ? new List<Expression> { value, origin }
                : new List<Expression> { value }
             ) { reference = reference };
        }

        /*
                public Expression translate(Node expression, Scope scope = null)
        {
            var swamp = new Swamp(this);
            return swamp.translate(expression, scope);
        }
        public Expression convert_path(IList<metahub.logic.nodes.Node> path, Scope scope = null)
        {
            var swamp = new Swamp(this);
            return swamp.convert_path(path, scope);
        }
        */

    }
}
