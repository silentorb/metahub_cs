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

        public Symbol create_symbol(string name, Profession profession)
        {
            // It is possible for a snippet to refer to define to a variable that only exists
            // within the scope of that snippet but shares the same name as a variable outside
            // of that scope.  In such a case, a mapping is tracked between the local name
            // of the variable and the actual rendered name of the variable.
            var actual_name = name;
            if (exists(name))
            {
                var i = 2;
                while (exists(actual_name + i))
                {
                    ++i;
                }

                actual_name = actual_name + i;

                // Sometimes there can be a series of snippets declaring the same variable.
                // Because of this each local symbol is also created in the parent scope
                // so the successive variable names can each be aware of the previous
                // variable names and thus all be unique.
                var first_symbol = find_or_null(name);
                first_symbol.scope.create_symbol(actual_name, profession);

                if (symbols.ContainsKey(name))
                    name = actual_name;
            }
            var symbol = new Symbol(actual_name, profession, this);
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