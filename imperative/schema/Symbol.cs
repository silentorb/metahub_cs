using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic.schema;

namespace metahub.imperative.schema
{
    public class Symbol
    {
        public string name;
        public Signature signature;
        public Scope scope;

        public Symbol(string name, Signature signature, Scope scope)
        {
            this.name = name;
            this.signature = signature;
            this.scope = scope;
        }
    }
}
