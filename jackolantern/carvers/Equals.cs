﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.jackolantern.tools;
using metahub.logic.nodes;

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
                //if (endpoint.portal.name == "position" || endpoint.portal.name == "radius")
                //    continue;

                var portal = endpoint.portal;
                var dungeon = portal.dungeon;
                var setter = jack.get_setter(portal);
                setter.block.add("post", new Comment("Carving equals: " + portal.name));
                var context = new Summoner.Context(setter);
                var swamp = new Swamp(jack, pumpkin, context);

                var original_target = swamp.get_exclusive_chain(endpoint.node, null, Dir.In);
                var transform = Transform.center_on(original_target.Last().node);
                var lvalue = transform.get_out(endpoint.node);
                var new_target = transform.get_out(original_target.Last().node);
                var rvalue = transform.get_out(pumpkin).get_other_input(new_target);
                //var rvalue = new_target.outputs[0].get_other_input(new_target);

                var parent = lvalue.inputs[0];

                context.add_pattern("first", swamp.translate_exclusive(parent, lvalue, Dir.In));
                context.add_pattern("second", swamp.translate_backwards(rvalue, null));
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }
    }
}
