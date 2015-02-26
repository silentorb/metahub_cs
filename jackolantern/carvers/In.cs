using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.summoner;
using metahub.imperative.expressions;
using metahub.jackolantern.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.carvers
{
    internal class In : Carver
    {
        public In(JackOLantern jack)
            : base(jack)
        {
        }

        public override void carve(Function_Node pumpkin)
        {
            var endpoints = get_endpoints3(pumpkin);
            foreach (var endpoint in endpoints)
            {
                var swamp = new Swamp(jack, pumpkin, null);
                var inputs = Swamp.get_inputs_in_relation_to(pumpkin, endpoint.node);
                var chain = swamp.get_inclusive_chain(inputs[0], null, Dir.In);

                var range = inputs[1].inputs.Select(i => swamp.translate_backwards(i, null)).ToArray();
                add_constraint(endpoint, get_additional_path(swamp, chain, 2), range);
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

        private void add_initialize(Endpoint endpoint, Expression property_expression, Expression[] range)
        {
            var initialize = jack.get_initialize(endpoint.portal.dungeon);
            var context = new Summoner.Context(initialize);
            context.set_pattern("prop", property_expression);
            context.set_pattern("min", range[0]);
            context.set_pattern("max", range[1]);
            var template = jack.templates["initialize_random_range"];
            initialize.accordian.add("pre", jack.overlord.summon_snippet(template, context));

        }

        private void add_constraint(Endpoint endpoint, Expression additional_path, Expression[] range)
        {
            var setter = jack.get_setter(endpoint.portal);
            var context = new Summoner.Context(setter);
            var value_expression = new Variable(setter.scope.find_or_exception("value"), additional_path);

            var template = jack.templates["value_constraint_check"];

            context.set_pattern("first", value_expression);

            var op = "<";

            for (var i = 0; i < 2; ++i)
            {
                context.set_pattern("$op", op);
                context.set_pattern("second", range[i]);
                context.set_pattern("third", range[i]);
                setter.accordian.add("pre", jack.overlord.summon_snippet(template, context));
                op = ">";
            }
        }
    }
}
