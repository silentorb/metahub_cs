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
            var endpoints = get_endpoints3(pumpkin);
            if (aggregate(pumpkin).Any(n => n.type == Node_Type.function_call))
                return;

            var first = pumpkin.inputs[0];
            var second = pumpkin.inputs[1];

            generate(pumpkin, first, second);
            generate(pumpkin, second, first);
        }

        void generate(Function_Call2 pumpkin, Node first, Node second)
        {
            if (aggregate(first).Any(n => n.type == Node_Type.function_call))
                return;

            var endpoints = get_endpoints4(first);
            foreach (var endpoint in endpoints)
            {
                var dungeon = endpoint.portal.dungeon;
                var setter = jack.get_setter(endpoint.portal);
                setter.block.add("post", new Comment("Carving equals: " + endpoint.portal.name));
                var context = new Summoner.Context(dungeon);
                context.scope = setter.scope;
                var swamp = new Swamp(jack, pumpkin);
                context.add_pattern("first", () => swamp.translate(second.inputs[0], null, Swamp.Dir.In, context.scope));
                context.add_pattern("second", () => swamp.translate(first.inputs[0], null, Swamp.Dir.In, context.scope));
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }
    }
}
