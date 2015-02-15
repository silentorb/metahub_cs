using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.carvers
{
    public class Contains : Carver
    {
        public Contains(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Node pumpkin)
        {
            var list = pumpkin.inputs[0];
            var item = pumpkin.inputs[1];

            var endpoints = get_endpoints(list, true);
            foreach (var endpoint in endpoints)
            {
                var dungeon = endpoint.dungeon;
                var initialize = jack.get_initialize(endpoint.dungeon);
                var context = new Summoner.Context(initialize);
                var swamp = new Swamp(jack, pumpkin, context);
                context.set_pattern("list", c => swamp.translate_inclusive(list, null, Dir.Out));
                context.set_pattern("item", c => swamp.translate_inclusive(item, null, Dir.Out));
                initialize.block.add(jack.overlord.summon_snippet(jack.templates["contains"], context));
            }
        }
    }
}
