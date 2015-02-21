using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;

namespace metahub.jackolantern.code
{
    public static class Portal_Carver
    {

        public static void customize_initialize(JackOLantern jack, Portal portal, Block block)
        {
            var tie = jack.get_tie(portal);
            if (tie == null)
                return;

            //foreach (Range_Float range in portal.tie.ranges)
            //{
            //    var reference = portal.create_reference(range.path.Count > 0
            //        ? new Path(range.path.Select(t => new Portal_Expression(portal.dungeon.overlord.get_portal(t))))
            //        : null
            //        );
            //    block.add(new Assignment(reference, "=", new Platform_Function("rand", null,
            //        new Expression[]
            //        {
            //            new Literal(range.min),
            //                        new Literal(range.max)
            //        })));
            //}

            if (tie.has_setter())
            {
                block.add("post", new Property_Function_Call(Property_Function_Type.set, portal, new List<Expression>
                    {
                        new Portal_Expression(portal)
                    }));
            }
        }

    }
}
