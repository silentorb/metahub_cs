using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.expressions;
using metahub.imperative.schema;
using metahub.render;

namespace metahub.jackolantern.code
{
    public class Dwarf
    {
        public Dwarf_Clan clan;
        public Minion minion;
        public Dungeon dungeon { get { return clan.dungeon; } }
        public Dictionary<string, Ration> rations = new Dictionary<string, Ration>();
        private Operation null_check;

        public Dwarf(Dwarf_Clan clan, Minion minion)
        {
            this.clan = clan;
            this.minion = minion;

            minion.on_add_expression += (minion_on_add);
        }

        void minion_on_add(Minion minion, Expression expression)
        {
            return;
            var portal_expressions = expression.aggregate()
                .OfType<Portal_Expression>()
                .Where(e => e.parent == null);

            foreach (var e in portal_expressions)
            {
                add_ration(e);
            }
        }

        void add_ration(Portal_Expression expression)
        {
            var chain = Ration.get_portal_path(expression);
            var key = chain.join(".");
            if (!rations.ContainsKey(key))
                rations[key] = new Ration(this, render_chain(chain));

            var ration = rations[key];
            ration.expressions.AddRange(chain);
            //if (null_check != null)
            //{
            //    null_check.add(ration.create_null_check());
            //}
        }

        static Expression render_chain(List<Expression> chain)
        {
            var result = chain[0].clone();
            var last = result;

            foreach (var link in chain.Skip(1))
            {
                last.next = link.clone();
            }

            return result;
        }

        public Operation get_null_check()
        {
            if (null_check == null)
            {
                var expressions = rations.Values.Select(r => r.create_null_check());
                null_check = new Operation("&&", expressions);
            }

            return null_check;
        }
    }
}
