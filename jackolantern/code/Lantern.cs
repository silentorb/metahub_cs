using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;

namespace metahub.jackolantern.code
{
    static class Lantern
    {
        public static Summoner.Context prepare_add_to_list(Expression list_expression, Portal portal, Profession profession, JackOLantern jack)
        {
            var setter = jack.get_setter(portal);
            var context = new Summoner.Context(setter);
            context.set_pattern("T", profession);
            context.set_pattern("list", list_expression);
            context.set_pattern("origin", new Self(setter.dungeon));
            context.set_pattern("additional", new Statements(new List<Expression>()));

            if (setter.parameters.Count > 0)
            {
                context.set_pattern("hub", new Portal_Expression(setter.dungeon.all_portals["hub"]));
            }
            else
            {
                context.set_pattern("hub", "");
            }

            return context;
        }

        public static Expression add_to_list(Expression list_expression, Portal portal, Profession profession, JackOLantern jack)
        {
            var context = prepare_add_to_list(list_expression, portal, profession, jack);
            return jack.overlord.summon_snippet(jack.templates["add_to_list"], context);
        }
    }
}
