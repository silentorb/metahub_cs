using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.types;
using metahub.logic.schema;

namespace metahub.imperative.schema
{
    public class Imp
    {
        public string name;
        public Dungeon dungeon;
        public Portal portal;
        public bool is_platform_specific;
        public List<Imp> invokers = new List<Imp>();
        public List<Imp> invokees = new List<Imp>();
        public List<Parameter> parameters;
        public List<Expression> expressions;
        public Signature return_type;
        public Imp parent;
        public List<Imp> children = new List<Imp>();

        public Imp(string name, Dungeon dungeon, Portal portal = null)
        {
            this.name = name;
            this.dungeon = dungeon;
            this.portal = portal;
        }

        public Function_Call invoke(Imp invoker, IEnumerable<Expression> args = null)
        {
            var invocation = new Function_Call(this, args);

            if (invoker != null)
            {
                if (!invoker.invokees.Contains(this))
                    invoker.invokees.Add(this);

                if (!invokers.Contains(invoker))
                    invokers.Add(invoker);
            }

            return invocation;
        }
    }
}
