using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.logic.schema;

namespace metahub.imperative.types
{

    public class Function_Definition : Anonymous_Function
    {
        public string name;
        public Dungeon dungeon;
        public Rail rail;
        public Scope scope;

        public Function_Definition(string name, Dungeon dungeon, List<Parameter> parameters, List<Expression> expressions, Signature return_type = null)
            : base(parameters, expressions, return_type)
        {
            this.name = name;
            this.dungeon = dungeon;
            this.rail = dungeon.rail;
            if (rail != null)
                dungeon.functions.Add(this);
        }
    }

}