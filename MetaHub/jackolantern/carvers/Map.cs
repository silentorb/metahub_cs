﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern.code;
using metahub.jackolantern.schema;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.schema;
using vineyard.transform;

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
            var transform = new Transform(pumpkin).initialize_map();

            var first = (Property_Node)transform.root.inputs[0];
            var second = (Property_Node)transform.root.inputs[1];

            var first_portal = jack.get_portal(first.property);
            var second_portal = jack.get_portal(second.property);

            on_add_code(first, first_portal, second_portal);
            on_add_code(second, second_portal, first_portal);
        }

        void on_add_code(Node list, Portal first_portal, Portal second_portal)
        {
            var first_list_portal = jack.get_portal(((Property_Node)list).property);

            var setter = jack.get_setter(first_list_portal);
            var context = new Summoner_Context(setter);
            var swamp = new Swamp(jack, null, context);
            var chain = swamp.get_exclusive_chain(list.inputs[0], list, Dir.In);
            var ref_expression = swamp.render_chain(chain.Take(chain.Count - 1).ToList());
            var list_expression = swamp.translate_exclusive(list.inputs[0], list, Dir.In);
            context.set_pattern("ref", ref_expression);
            var portal = jack.get_portal(((Property_Node) list).property);
            context.set_pattern("$add", on_add_code_sub(list_expression, portal, first_portal, second_portal,setter));
            setter.accordian.add("post", jack.overlord.summon_snippet(jack.templates["map_on_add"], context));
        }

        Expression on_add_code_sub(Expression list_expression, Portal portal, Portal first_portal, Portal second_portal, Minion setter)
        {
            var context = Lantern.prepare_add_to_list(list_expression, portal, first_portal.profession, jack);
            context.set_pattern("main_item", new Variable(setter.scope.find_or_exception("item")));
            context.set_pattern("$link", new Portal_Expression(second_portal));
            return jack.summon_snippet("map_add_to_list", context);
        }

        static void point_at(Portal pointer, Portal target)
        {
            pointer.other_portal = target;
        }

        Portal create_reference(Property_Node target, Property_Node other)
        {
            var first_type = target.get_signature();
            var other_rail = other.get_signature().trellis;
            var first_dungeon = jack.get_dungeon_or_error(first_type.trellis);
            var other_dungeon = jack.get_dungeon_or_error(other_rail);
            var first_name = first_dungeon.get_available_name("map_" + other_dungeon.name.ToLower(), 1);
            var portal = first_dungeon.add_portal(new Portal(first_name, Profession.create(other_dungeon)));
            var tie = new Property(first_name, Kind.reference, first_type.trellis, other_rail);
            var rail = jack.get_rail(first_dungeon);
            rail.all_properties[tie.name] = tie;
            rail.core_properties[tie.name] = tie;
            jack.clans[first_dungeon].generate_setter(portal);
            return portal;
        }
    }
}
