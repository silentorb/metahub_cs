using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.logic.nodes;

namespace metahub.imperative.types
{
    public class Class_Function_Call : Function_Call
    {
        public string name;
        public Imp imp;
        public Profession profession;

        public Class_Function_Call(string name, Expression reference = null, IEnumerable<Expression> args = null)
            : base(Expression_Type.function_call, reference, args)
        {
            this.name = name;
        }

        public Class_Function_Call(Imp imp, Expression reference = null, IEnumerable<Expression> args = null)
            : base(Expression_Type.function_call, reference, args)
        {
            this.imp = imp;
            name = imp.name;
        }

        public Class_Function_Call set_reference(Expression reference)
        {
            this.reference = reference;
            return this;
        }

        public override Profession get_profession()
        {
            return imp != null
                ? new Profession(imp.return_type, imp.dungeon.overlord)
                : profession;
        }

        public override Expression clone()
        {
            return new Class_Function_Call(imp, reference, args);
        }
    }

}