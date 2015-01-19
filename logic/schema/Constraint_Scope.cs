using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic.types;

namespace metahub.logic.schema
{
    public class Constraint_Scope
    {
        public string name;
        public Node[] caller;

        public Constraint_Scope(string name, Node[] caller)
        {
            this.name = name;
            this.caller = caller;
        }
    }
}
