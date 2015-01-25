using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.types;
using metahub.logic.nodes;

namespace metahub.jackolantern.pumpkins
{
    public class Contains : Carver
    {
        public Contains(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Call2 pumpkin)
        {
            var list = pumpkin.inputs[0];
            var item = pumpkin.inputs[1];

            var endpoints = get_endpoints(pumpkin);
            foreach (var endpoint in endpoints)
            {
                var initialize = jack.get_initialize(endpoint.dungeon);
                initialize.expressions.Add(new Comment("Carving contains : " + endpoint.name));
            }
        }
    }
}
