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
        public Profession profession;
        public Scope scope;

        public Symbol(string name, Signature signature, Scope scope)
        {
            this.name = name;
            this.signature = signature;
            this.scope = scope;
        }

        public Symbol(string name, Profession profession, Scope scope)
        {
            this.name = name;
            this.profession = profession;
            this.scope = scope;
        }
    }
}
