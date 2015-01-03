using System;
using System.Collections.Generic;
using metahub.logic.schema;

namespace metahub.imperative.schema
{
    public class Scope
    {
        public Scope parent;
        Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        public Scope(Scope parent = null)
        {
            this.parent = parent;
        }

        public Symbol create_symbol(string name, Signature signature)
        {
            if (exists(name))
            {
                var i = 2;
                while (exists(name + i))
                {
                    ++i;
                }

                name = name + i;
            }
            var symbol = new Symbol(name, signature, this);
            symbols[name] = symbol;
            return symbol;
        }

        public bool exists(string name)
        {
            return symbols.ContainsKey(name) || (parent != null && parent.exists(name));
        }

        public Symbol find(string name)
        {
            if (symbols.ContainsKey(name))
                return symbols[name];

            if (parent != null)
                return parent.find(name);

            throw new Exception("Could not find symbol " + name + ".");
            //return null;
        }
    }
}