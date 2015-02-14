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
        class Process
        {
            public Endpoint endpoint;
            public Transform transform;
            public Imp setter;
            public Swamp swamp;
            public List<Node_Link> target;
        }

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

            var processes = endpoints.Select(endpoint =>
                {
                    var portal = endpoint.portal;
                    var dungeon = portal.dungeon;
                    var setter = jack.get_setter(portal);
                    var context = new Summoner.Context(setter); 
                    var swamp = new Swamp(jack, pumpkin, context);
                    var original_target = swamp.get_exclusive_chain(endpoint.node, null, Dir.In);
                    return new Process()
                        {
                            transform = Transform.center_on(original_target.Last().node),
                            endpoint = endpoint,
                            setter = setter,
                            swamp = swamp,
                            target = original_target
                        };
                }).ToArray();

            var has_transforms = processes.Any(p => p.transform.map.Count > 0);

            var i = 0;
            foreach (var process in processes)
            {
                //if (endpoint.portal.name == "position" || endpoint.portal.name == "radius")
                //    continue;

                var setter = process.setter;
                var transform = process.transform;
                var swamp = process.swamp;
                var context = swamp.context;
                var endpoint = process.endpoint;

                //var original_target = swamp.get_exclusive_chain(endpoint.node, null, Dir.In);

                //var transform = Transform.center_on(original_target.Last().node);
                var new_target = transform.get_transformed(process.target.Last().node);
                var lvalue = transform.get_transformed(endpoint.node);
                var rvalue = transform.get_transformed(pumpkin).get_other_input(new_target);
                var parent = lvalue.inputs[0];
                var lexpression = swamp.translate_exclusive(parent, lvalue, Dir.In);
                var rexpression = swamp.translate_backwards(rvalue, null);
                if (has_transforms)
                {
                    context.add_pattern("first", lexpression);
                    context.add_pattern("second", rexpression);
                }
                else
                {
                    context.add_pattern("second", lexpression);
                    context.add_pattern("first", rexpression);
                }

                setter.block.add("post", new Comment("Carving equals: " + endpoint.portal.name + ": " 
                    + swamp.get_exclusive_chain(parent, lvalue, Dir.In).Select(n=>n.node.debug_string).join(" <- ")));
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }
    }
}
