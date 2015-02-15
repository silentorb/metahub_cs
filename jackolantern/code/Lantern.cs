using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.code
{
    static class Lantern
    {
        public static Expression add_to_list(Expression list_expression, Portal portal, Profession profession, JackOLantern jack)
        {
            var setter = jack.get_setter(portal);
            var context = new Summoner.Context(setter);
            context.set_pattern("T", profession);
            context.set_pattern("list", list_expression);

            if (setter.parameters.Count > 0)
            {
                context.set_pattern("hub", new Variable(setter.parameters[0].symbol));
            }
            else
            {
                context.set_pattern("hub", "");
            }

            return jack.overlord.summon_snippet(jack.templates["add_to_list"], context);
        }
    }
}
