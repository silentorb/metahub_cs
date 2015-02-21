using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using metahub.imperative;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.summoner;
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

        public JackOLantern(Logician logician, Overlord overlord)
        {
            this.logician = logician;
            this.overlord = overlord;
            initialize_functions();
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

        public void generate_code(Target target)
        {
            foreach (var region in overlord.railway.regions.Values)
            {
                //if (region.is_external)
                //    continue;

                var realm = new Realm(region, overlord);
                overlord.realms[realm.name] = realm;

                foreach (var rail in region.rails.Values)
                {

                    Dungeon dungeon = new Dungeon(rail, overlord, realm);
                    realm.dungeons[dungeon.name] = dungeon;
                    overlord.rail_map[rail] = dungeon;

                    if (rail.trellis.is_abstract && rail.trellis.is_value)
                        continue;

                    overlord.dungeons.Add(dungeon);
                }
            }

            foreach (var dungeon in overlord.dungeons)
            {
                dungeon.initialize();
            }

            overlord.finalize();

            var not_external = overlord.dungeons.Where(d => !d.is_external).ToArray();

            foreach (var dungeon in not_external)
            {
                dungeon.generate_code();
                Dungeon_Carver.generate_code1(this, dungeon, dungeon.rail);
            }

            foreach (var dungeon in not_external)
            {
                target.generate_rail_code(dungeon);
                Dungeon_Carver.generate_code2(this, dungeon, dungeon.rail);
            }

            overlord.summon(Resources.metahub_imp);

            if (logician.railway.regions.ContainsKey("piecemaker"))
            {
                var piece_region = logician.railway.regions["piecemaker"];
                Piece_Maker.add_functions(overlord, piece_region);
            }
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

        public Property_Function_Call call_setter(Portal portal,Expression reference, Expression value, Expression origin)
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
