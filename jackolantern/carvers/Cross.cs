using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
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
            var portal = jack.get_portal(property_node.tie);
            var setter = jack.get_setter(portal);
            var lambda = (Lambda)pumpkin.inputs[1];

            foreach (Variable_Node variable in pumpkin.outputs)
            {
                variable.name = variable.name == lambda.parameter_names[0]
                    ? "this"
                    : "other";
            }
            
            foreach (Function_Node constraint in constraints)
            {
                var minion = create_check_function(portal);

                // Iterator
                var context = new Summoner.Context(setter);
                context.scope.add_map("this", c=> new Self(portal.other_dungeon));
                context.set_pattern("list", new Portal_Expression(portal.other_portal, new Portal_Expression(portal)));
                context.set_pattern("condition", c=> render_inverse_constraint(constraint, c));
                context.set_pattern("block", c =>
                    {
                        var conflict_class = Piece_Maker.create_conflict_class(setter.dungeon, jack);
                        c.set_pattern("T", setter.parameters[0].symbol.profession);
                        c.set_pattern("T2", new Profession(Kind.reference, conflict_class));

                        jack.overlord.summon_snippet(
                            Piece_Maker.templates["create_distance_conflict"], c);
                    });

                var body = jack.summon_snippet("cross_iterator", context);
                minion.add_to_block(body);
            }
        }

        Minion create_check_function(Portal portal)
        {
            var dungeon = portal.other_dungeon;
            var minion_name = dungeon.get_available_name("check_" + portal.name + "_cross", 1);
            var minion = dungeon.spawn_minion(minion_name);
            minion.return_type = new Profession(Kind.Bool);
            var profession = new Profession(Kind.reference, portal.other_dungeon);
            minion.add_parameter("value", profession);
            return minion;
        }

        Expression render_inverse_constraint(Function_Node constraint, Summoner.Context context)
        {
            var swamp = new Swamp(jack, null, context);
            var first = swamp.translate_backwards(constraint.inputs[0], constraint);
            var second = swamp.translate_backwards(constraint.inputs[0], constraint);
            return new Operation(Logician.inverse_operators[constraint.name], first, second);
        }

    }
}
