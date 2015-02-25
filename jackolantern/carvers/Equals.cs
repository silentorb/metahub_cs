using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.summoner;
using metahub.imperative.expressions;
using metahub.jackolantern.schema;
using metahub.logic.nodes;
using metahub.render;
using metahub.schema;

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

            var endpoints = get_endpoints3(pumpkin);
            foreach (var endpoint in endpoints)
            {
                var portal = endpoint.portal;
                var setter = jack.get_setter(portal);
                var context = new Summoner.Context(setter);
                var swamp = new Swamp(jack, pumpkin, context);
                var expressions = swamp.get_expression_pair(endpoint.node);

                setter.block.add("post", new Comment("Carving equals: " + endpoint.portal.name));
                var conditions = get_conditions(expressions[0]);
                if (portal.type == Kind.reference || portal.type == Kind.list)
                    expressions = expressions.Reverse().ToArray();

                if (expressions[0] != null && expressions[0].type == Expression_Type.operation)
                    throw new Exception("Cannot call function on operation.");

                context.set_pattern("condition", conditions);
                context.set_pattern("first", expressions[0]);
                context.set_pattern("second", expressions[1]);
                setter.block.add("post", jack.overlord.summon_snippet(jack.templates["equals"], context));
            }
        }

        //static Operation get_conditions(Expression start)
        //{
        //    var expression = start;
        //    var conditions = new List<Expression>();
        //    if (expression.child == null)
        //        throw new Exception("Child expression cannot be null.");

        //    do
        //    {
        //        expression = expression.clone();
        //        expression.get_end().disconnect_parent();
        //        conditions.Insert(0, new Operation("!=", new[] { expression, new Null_Value() }));
        //    }
        //    while (expression.child != null);

        //    return new Operation("&&", conditions);
        //}
    }
}
