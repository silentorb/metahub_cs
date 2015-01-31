using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.pumpkins
{
    public class Equals : Carver
    {
        public Equals(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Call2 pumpkin)
        {
            //var endpoints = get_endpoints3(pumpkin);
            if (aggregate(pumpkin).Any(n => n.type == Node_Type.function_call))
                return;

            //var first = pumpkin.inputs[0];
            //var second = pumpkin.inputs[1];

            //generate(pumpkin, first, second);
            //generate(pumpkin, second, first);

            //if (aggregate(first).Any(n => n.type == Node_Type.function_call))
            //    return;

            var endpoints = get_endpoints3(pumpkin, false);
            foreach (var endpoint in endpoints)
            {
                //if (endpoint.portal.name != "dir")
                //    continue;

                var dungeon = endpoint.portal.dungeon;
                var setter = jack.get_setter(endpoint.portal);
                setter.block.add("post", new Comment("Carving equals: " + endpoint.portal.name));
                var context = new Summoner.Context(dungeon);
                context.scope = setter.scope;
                var swamp = new Swamp(jack, pumpkin);
                var node = endpoint.node;
                var parent = node.inputs[0];
                //swamp.translate2(parent, node, Swamp.Dir.In, context);
                context.add_pattern("first", () => swamp.translate2(parent, node, Swamp.Dir.In, context));
                context.add_pattern("second", () => swamp.translate2(parent, node, Swamp.Dir.Out, context));
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }
    }
}
