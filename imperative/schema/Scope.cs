using System;
using System.Collections.Generic;
using metahub.imperative.types;
using metahub.logic.schema;

namespace metahub.imperative.schema
{

    public class Scope
    {
        public Scope parent;
        Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
        Dictionary<string, Expression_Generator> map = new Dictionary<string, Expression_Generator>();

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

        public void add_map(string name, Expression_Generator generator)
        {
            map[name] = generator;
        }

        public bool exists(string name)
        {
            return symbols.ContainsKey(name) || (parent != null && parent.exists(name));
        }

        public Symbol find_or_null(string name)
        {
            if (symbols.ContainsKey(name))
                return symbols[name];

            if (parent != null)
                return parent.find_or_null(name);

            return null;
        }

        public Symbol find_or_exception(string name)
        {
            if (symbols.ContainsKey(name))
                return symbols[name];

            if (parent != null)
                return parent.find_or_exception(name);

            throw new Exception("Could not find symbol " + name + ".");
            //return null;
        }

        public Expression resolve(string name)
        {
            return map[name]();
        }
    }
}