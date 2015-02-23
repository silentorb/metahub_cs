using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;


namespace metahub.imperative.types
{

    public class Function_Definition : Anonymous_Function
    {
        public string name;
        public Dungeon dungeon;
        public Scope scope;
        public Minion minion;
        public Profession return_type { get { return minion.return_type; } }
        public bool is_abstract { get { return minion.is_abstract; } }
        public List<Expression> expressions { get { return minion.expressions; } }

        //public Function_Definition(string name, Dungeon dungeon, List<Parameter> parameters, List<Expression> expressions, Signature return_type = null)
        //    : base(parameters, expressions, return_type)
        //{
        //    this.name = name;
        //    this.dungeon = dungeon;
        //    rail = dungeon.rail;

        //    minion = new Minion(name, dungeon)
        //        {
        //            return_type = _return_type,
        //            expressions = expressions
        //        };
        //}

        public Function_Definition(Minion minion)
            : base(minion.parameters, minion.expressions, minion.return_type)
        {
            this.minion = minion;
            name = minion.name;
            dungeon = minion.dungeon;
        }
    }

}