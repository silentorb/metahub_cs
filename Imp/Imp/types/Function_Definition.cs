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
        public Imp imp;
        public Profession return_type { get { return imp.return_type; } }
        public bool is_abstract { get { return imp.is_abstract; } }
        public List<Expression> expressions { get { return imp.expressions; } }

        //public Function_Definition(string name, Dungeon dungeon, List<Parameter> parameters, List<Expression> expressions, Signature return_type = null)
        //    : base(parameters, expressions, return_type)
        //{
        //    this.name = name;
        //    this.dungeon = dungeon;
        //    rail = dungeon.rail;

        //    imp = new Imp(name, dungeon)
        //        {
        //            return_type = _return_type,
        //            expressions = expressions
        //        };
        //}

        public Function_Definition(Imp imp)
            : base(imp.parameters, imp.expressions, imp.return_type)
        {
            this.imp = imp;
            name = imp.name;
            dungeon = imp.dungeon;
        }
    }

}