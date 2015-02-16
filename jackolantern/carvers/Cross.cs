using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.code;
using metahub.jackolantern.schema;
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
            foreach (var constraint in constraints)
            {
                // Create check function
                var property_node = (Property_Node)pumpkin.inputs[0];
                var portal = jack.overlord.get_portal(property_node.tie);
                var setter = jack.get_setter(portal);
                var dungeon = portal.other_dungeon;
                var imp_name = dungeon.get_available_name("check_" + portal.name + "_cross", 1);
                var imp = dungeon.spawn_imp(imp_name);
                var profession = new Profession(Kind.reference, portal.other_dungeon);
                imp.add_parameter("value", profession);

                // Iterator
                var context = new Summoner.Context(setter);
                context.set_pattern("list", new Portal_Expression(portal.other_portal, new Portal_Expression(portal)));
                context.set_pattern("block", new Statements());
                var body = jack.overlord.summon_snippet(jack.templates["cross_iterator"], context);
                imp.add_to_block(body);
            }

        }

    }
}
