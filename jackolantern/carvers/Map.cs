using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.code;
using metahub.jackolantern.schema;
using metahub.jackolantern.tools;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.jackolantern.carvers
{
    public class Map : Carver
    {
        public Map(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Node pumpkin)
        {
            var first = (Property_Node)pumpkin.inputs[0];
            var second = (Property_Node)pumpkin.inputs[1];
            var lambda = (Lambda)pumpkin.inputs[2];

            var first_portal = create_reference(first, second);
            var second_portal = create_reference(second, first);

            point_at(first_portal, second_portal);
            point_at(second_portal, first_portal);

            var nodes = aggregate(pumpkin);

            var first_parameter = (Parameter_Node)lambda.inputs[0];
            var second_parameter = (Parameter_Node)lambda.inputs[1];

            var variables = pumpkin.outputs.OfType<Variable_Node>().ToArray();
            foreach (var node in variables)
            {
                if (node.name == first_parameter.name)
                {
                    node.replace(new Property_Node(first_portal.tie));
                }
                else if (node.name == second_parameter.name)
                {
                    node.replace(new Property_Node(second_portal.tie));
                }
                else
                {
                    throw new Exception("Could not find parameter " + node.name);
                }
            }

            var second_list_portal = jack.overlord.get_portal(second.tie);

            on_add_code(first, second, first_portal, second_portal);
            on_add_code(second, first, second_portal, first_portal);
        }

        void on_add_code(Node list, Node other_list, Portal first_portal, Portal second_portal)
        {
            var first_list_portal = jack.overlord.get_portal(((Property_Node)list).tie);

            var setter = jack.get_setter(first_list_portal);
            var context = new Summoner.Context(setter);
            var swamp = new Swamp(jack, null, context);

            context.add_pattern("T", first_portal.profession);
            {
                var chain = swamp.get_exclusive_chain(list.inputs[0], list, Dir.In);
                var ref_expression = swamp.render_chain(chain.Take(chain.Count - 1).ToList());
                context.add_pattern("ref", ref_expression);
            }
            context.add_pattern("list", swamp.translate_exclusive(list.inputs[0], list, Dir.In));
            if (setter.parameters.Count > 0)
            {
                context.add_pattern("hub", new Variable(setter.parameters[0].symbol));
            }
            else
            {
                context.add_pattern("hub", "");
            }
            setter.block.add("post", jack.overlord.summon_snippet(jack.templates["map_on_add"], context));
        }

        static void point_at(Portal pointer, Portal target)
        {
            pointer.other_portal = target;
            pointer.tie.other_tie = target.tie;
        }

        Portal create_reference(Property_Node target, Property_Node other)
        {
            var first_type = target.get_signature();
            var other_rail = other.get_signature().rail;
            var first_dungeon = jack.overlord.get_dungeon_or_error(first_type.rail);
            var other_dungeon = jack.overlord.get_dungeon_or_error(other_rail);
            var first_name = first_dungeon.get_available_name("map_" + other_dungeon.name.ToLower(), 1);
            var portal = first_dungeon.add_portal(new Portal(first_name, new Profession(Kind.reference, other_dungeon)));
            portal.tie = new Tie(first_name, first_type.rail, Kind.reference, other_rail);
            Dungeon_Carver.generate_setter_stub(portal);
            Dungeon_Carver.generate_setter(portal);
            return portal;
        }
    }
}
