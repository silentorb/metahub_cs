using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.types;
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
            var endpoints = get_endpoints(pumpkin);
            if (aggregate(pumpkin).Any(n => n.type == Node_Type.function_call))
                return;

            foreach (var endpoint in endpoints)
            {
                var setter = jack.get_setter(endpoint);
                setter.expressions.Add(new Comment("Carving equals"));
            }
        }
    }
}
