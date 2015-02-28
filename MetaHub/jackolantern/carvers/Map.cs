using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
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

            var first_parameter = (Parameter_Node)lambda.inputs[0];
            var second_parameter = (Parameter_Node)lambda.inputs[1];

            var variables = pumpkin.outputs.OfType<Variable_Node>().ToArray();

            foreach (var node in variables)
            {
                Property_Node property_node;
                if (node.name == first_parameter.name)
                {
                    property_node = new Property_Node(jack.get_tie(first_portal));
                }
                else if (node.name == second_parameter.name)
                {
                    property_node = new Property_Node(jack.get_tie(second_portal));
                }
                else
                {
                    throw new Exception("Could not find parameter " + node.name);
                }

                node.replace(property_node);
                property_node.disconnect(pumpkin);
            }

            on_add_code(first, second, first_portal, second_portal);
            on_add_code(second, first, second_portal, first_portal);
        }

        void on_add_code(Node list, Node other_list, Portal first_portal, Portal second_portal)
        {
            var first_list_portal = jack.get_portal(((Property_Node)list).tie);

            var setter = jack.get_setter(first_list_portal);
            var context = new Summoner.Context(setter);
            var swamp = new Swamp(jack, null, context);
            var chain = swamp.get_exclusive_chain(list.inputs[0], list, Dir.In);
            var ref_expression = swamp.render_chain(chain.Take(chain.Count - 1).ToList());
            var list_expression = swamp.translate_exclusive(list.inputs[0], list, Dir.In);
            context.set_pattern("ref", ref_expression);
            var portal = jack.get_portal(((Property_Node) list).tie);
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
            var other_rail = other.get_signature().rail;
            var first_dungeon = jack.get_dungeon_or_error(first_type.rail);
            var other_dungeon = jack.get_dungeon_or_error(other_rail);
            var first_name = first_dungeon.get_available_name("map_" + other_dungeon.name.ToLower(), 1);
            var portal = first_dungeon.add_portal(new Portal(first_name, new Profession(Kind.reference, other_dungeon)));
            var tie = new Property(first_name, Kind.reference, first_type.rail, other_rail);
            var rail = jack.get_rail(first_dungeon);
            rail.all_properties[tie.name] = tie;
            rail.core_properties[tie.name] = tie;
            jack.clans[first_dungeon].generate_setter(portal);
            return portal;
        }
    }
}
