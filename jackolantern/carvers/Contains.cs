﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.pumpkins
{
    public class Contains : Carver
    {
        public Contains(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Call2 pumpkin)
        {
            var list = pumpkin.inputs[0];
            var item = pumpkin.inputs[1];

            var endpoints = get_endpoints(list, true);
            foreach (var endpoint in endpoints)
            {
                var dungeon = endpoint.dungeon;
                var initialize = jack.get_initialize(endpoint.dungeon);
                var context = new Summoner.Context(dungeon);
                context.scope = initialize.scope;
                var swamp = new Swamp(jack, pumpkin, context);
                context.add_pattern("list", () => swamp.translate_inclusive(list, null, Dir.Out));
                context.add_pattern("item", () => swamp.translate_inclusive(item, null, Dir.Out));
                initialize.block.add(jack.overlord.summon_snippet(jack.templates["contains"], context));
            }
        }
    }
}
