using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.jackolantern.tools;
using metahub.logic.nodes;
using metahub.render;

namespace metahub.jackolantern.carvers
{
    public class Equals : Carver
    {
        public Equals(JackOLantern jack)
            : base(jack)
        {

        }

        public static string[] container_functions = new[]
            {
                "map"
            };

        public override void carve(Function_Node pumpkin)
        {
            //var endpoints = get_endpoints3(pumpkin);
            if (aggregate(pumpkin).OfType<Function_Node>().Any(n => !n.is_operation && !container_functions.Contains(n.name)))
                return;

            var endpoints = get_endpoints3(pumpkin, false);
            foreach (var endpoint in endpoints)
            {
                var portal = endpoint.portal;
                var setter = jack.get_setter(portal);
                var context = new Summoner.Context(setter);
                var swamp = new Swamp(jack, pumpkin, context);
                var expressions = swamp.get_expression_pair(endpoint.node);

                //setter.block.add("post", new Comment("Carving equals: " + endpoint.portal.name + ": " 
                //    + swamp.get_exclusive_chain(parent, lvalue, Dir.In).Select(n=>n.node.debug_string).join(" <- ")));
                context.set_pattern("first", expressions[0]);
                context.set_pattern("second", expressions[1]);
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }
    }
}
