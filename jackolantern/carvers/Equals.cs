using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.jackolantern.tools;
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
            if (aggregate(pumpkin).OfType<Function_Call2>().Any(n => !n.is_operation))
                return;

            var endpoints = get_endpoints3(pumpkin, false);
            foreach (var endpoint in endpoints)
            {
                //if (endpoint.portal.name != "dir")
                //    continue;
                var portal = endpoint.portal;
                var center = Transform.center_on(endpoint.node);

                var dungeon = portal.dungeon;
                var setter = jack.get_setter(portal);
                setter.block.add("post", new Comment("Carving equals: " + portal.name));
                var context = new Summoner.Context(dungeon);
                context.scope = setter.scope;
                var swamp = new Swamp(jack, pumpkin, context);
                var parent = center.inputs[0];
                //swamp.translate2(parent, node, Swamp.Dir.In, context);
                context.add_pattern("first", () => swamp.translate_exclusive(parent, center, Dir.In));
                context.add_pattern("second", () => swamp.translate_exclusive(parent, center, Dir.Out));
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }
    }
}
