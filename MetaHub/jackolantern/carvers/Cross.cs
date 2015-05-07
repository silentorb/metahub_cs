using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.code;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern.code;
using metahub.jackolantern.schema;
using metahub.logic;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.jackolantern.carvers
{
    public class Cross : Carver
    {
        public Cross(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Node pumpkin)
        {
            var constraints = pumpkin.outputs.Select(o => o.get_last(Dir.Out)).Distinct();

            var property_node = (Property_Node)pumpkin.inputs[0];
            var portal = jack.get_portal(property_node.property);
            var lambda = (Lambda)pumpkin.inputs[1];

            foreach (Variable_Node variable in pumpkin.outputs)
            {
                variable.name = variable.name == lambda.parameter_names[0]
                    ? "this"
                    : "other";
            }

            var conflict_class = Piece_Maker.create_conflict_class((Dungeon)portal.other_dungeon, jack);
            var setter = portal.setter;
            foreach (Function_Node constraint in constraints)
            {
                var endpoints = get_endpoints3(constraint).Where(e => e.portal != portal);

                var used_portals = new Dictionary<Portal, Endpoint>();
                foreach (var endpoint in endpoints)
                {
                    if (used_portals.ContainsKey(endpoint.portal))
                        continue;

                    used_portals[endpoint.portal] = endpoint;

                    var minion = process_endpoint(portal, constraint, endpoint, conflict_class);
                    endpoint.portal.setter.add_to_block("post", new Method_Call(minion, null,
                        new Variable(endpoint.portal.setter.parameters[0].symbol)));
                }
            }
        }

        Minion create_check_function(Portal portal, Endpoint endpoint)
        {
            var dungeon = (Dungeon)portal.other_dungeon;
            var minion_name = dungeon.get_available_name("check_cross_" + endpoint.portal.name, 1);
            var minion = dungeon.spawn_minion(minion_name);
            minion.return_type = new Profession(Kind.Bool);
            //var profession = new Profession(Kind.reference, endpoint.portal.other_dungeon);
            minion.add_parameter("value", endpoint.portal.get_target_profession());
            return minion;
        }

        Expression render_inverse_constraint(Function_Node constraint, Summoner_Context context)
        {
            var swamp = new Swamp(jack, null, context);
            var first = swamp.translate_backwards(constraint.inputs[0], constraint);
            var second = swamp.translate_backwards(constraint.inputs[1], constraint);
            return new Operation(Logician.inverse_operators[constraint.name], first, second);
        }

        Minion process_endpoint(Portal portal, Function_Node constraint, Endpoint endpoint, Dungeon conflict_class)
        {
            var minion = create_check_function(portal, endpoint);

            // Iterator
            var context = new Summoner_Context(minion);
            //context.scope.add_map("this", c => new Self(portal.other_dungeon));
            context.set_pattern("list", new Portal_Expression(portal.other_portal, new Portal_Expression(portal)));
            context.set_pattern("condition", c => render_inverse_constraint(constraint, c));
            context.set_pattern("block", c => generate_response(c, endpoint.portal, constraint, conflict_class));

            var dwarf = jack.get_dwarf(minion);
            context.set_pattern("null_check", dwarf.get_null_check());
            
            var body = jack.summon_snippet("cross_iterator", context);
            minion.add_to_block(body);
            return minion;
        }

        Expression generate_response(Summoner_Context context, Portal portal, Function_Node constraint, Dungeon conflict_class)
        {
            context.set_pattern("T", portal.profession);
            context.set_pattern("T2", new Profession(Kind.reference, conflict_class));
            //c.set_pattern("T2", new Profession(Kind.reference, conflict_class));
            //c.set_pattern("T2", new Profession(Kind.reference, conflict_class));
            var swamp = new Swamp(jack, null, context);
            var distance = constraint.inputs[0];
            context.set_pattern("other", swamp.translate_backwards(distance.inputs[1].inputs[0], distance));
            context.set_pattern("other_path", swamp.translate_backwards(distance.inputs[1], distance));
            context.imported_realms.Add(jack.overlord.root.get_realm(new[] { "Microsoft", "Xna", "Framework" }));

            return jack.overlord.summon_snippet(
                Piece_Maker.templates["create_distance_conflict"], context);
        }
    }
}
