﻿using System;
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

            foreach (var node in nodes.OfType<Variable_Node>())
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

            var first_list_portal = jack.overlord.get_portal(first.tie);
            var second_list_portal = jack.overlord.get_portal(second.tie);

            on_add_code(first_list_portal, second_list_portal, first_portal, second_portal);
            on_add_code(second_list_portal, first_list_portal, second_portal, first_portal);
        }

        void on_add_code(Portal list, Portal other_list, Portal first_portal, Portal second_portal)
        {
            var setter = jack.get_setter(list);
            var context = new Summoner.Context(setter);
            context.add_pattern("T", second_portal.profession);
            context.add_pattern("list", new Portal_Expression(other_list));
            string template_name;
            if (setter.parameters.Count > 0)
            {
                context.add_pattern("hub", new Variable(setter.parameters[0].symbol));
                template_name = "map_on_add_with_hub";
            }
            else
            {
                template_name = "map_on_add";
            }
            setter.block.add("post", jack.overlord.summon_snippet(jack.templates[template_name], context));
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
