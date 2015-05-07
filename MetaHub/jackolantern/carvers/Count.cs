using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern.code;
using metahub.jackolantern.schema;
using metahub.logic.nodes;
using vineyard.transform;

namespace metahub.jackolantern.carvers
{
    class Count : Carver
    {
           public Count(JackOLantern jack)
            : base(jack)
        {
        }

        public override void carve(Function_Node pumpkin)
        {
            var endpoints = get_endpoints3(pumpkin);
            foreach (var endpoint in endpoints)
            {
                var swamp = new Swamp(jack, pumpkin, null);
                //var inputs = Swamp.get_inputs_in_relation_to(pumpkin, endpoint.node);
                var chain = swamp.get_inclusive_chain(pumpkin.inputs[0], null, Dir.In);

                var equals = pumpkin.outputs[0];
                var range = swamp.translate_backwards(equals.get_other_input(pumpkin), null);
                add_initialize(endpoint, get_additional_path(swamp, chain, 1, true), range);
            }
        }

        Expression get_additional_path(Swamp swamp, List<Node_Link> chain, int skip, bool reverse = false)
        {
            if (chain.Count - skip < 1)
                return null;

            var new_chain = chain.Take(chain.Count - skip).ToList();
            foreach (var node_link in new_chain)
            {
                node_link.dir = Dir.Out;
            }

            if (reverse)
                new_chain.Reverse();

            return swamp.render_chain(new_chain);
        }

        private void add_initialize(Endpoint endpoint, Expression property_expression, Expression range)
        {
            var initialize = jack.get_initialize(endpoint.portal.dungeon);
            var context = new Summoner_Context(initialize);

            context.set_pattern("list", property_expression);
            context.set_pattern("size", range);
            var profession = endpoint.portal.profession.clone();
            profession.is_list = false;
            context.set_pattern("$add", Lantern.add_to_list(property_expression, endpoint.portal, profession, jack));
            var template = jack.templates["initialize_count"];
            initialize.accordian.add("pre", jack.overlord.summon_snippet(template, context));

        }

    }
}
