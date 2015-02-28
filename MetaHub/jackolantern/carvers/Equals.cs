using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.summoner;
using imperative.expressions;
using metahub.jackolantern.schema;
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

            var endpoints = get_endpoints3(pumpkin).ToArray();
            var pivots = endpoints.Where(e => e.node.outputs[0].type != Node_Type.property).ToArray();
            foreach (var endpoint in endpoints)
            {
                foreach (var other in pivots)
                {
                    if (endpoint == other)
                        continue;

                    var portal = endpoint.portal;
                    var setter = jack.get_setter(portal);
                    var context = new Summoner.Context(setter);
                    var swamp = new Swamp(jack, pumpkin, context);
                    var expressions = swamp.get_expression_pair(other.node);

                    setter.accordian.add("post", new Comment("Carving equals: " + endpoint.portal.name));
                    var conditions = get_conditions(expressions[0]);
                    //if (portal.type == Kind.reference || portal.type == Kind.list)
                    //expressions = expressions.Reverse().ToArray();

                    if (expressions[0] != null && expressions[0].type == Expression_Type.operation)
                        throw new Exception("Cannot call function on operation.");

                    context.set_pattern("condition", conditions);
                    context.set_pattern("first", expressions[0]);
                    context.set_pattern("second", expressions[1]);
                    setter.accordian.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
                }
            }
        }
    }
}
