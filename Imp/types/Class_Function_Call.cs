using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;


namespace metahub.imperative.types
{
    public class Class_Function_Call : Function_Call
    {
        public string name;
        public Minion minion;
        public Profession profession;

        public Class_Function_Call(string name, Expression reference = null, IEnumerable<Expression> args = null)
            : base(Expression_Type.function_call, reference, args)
        {
            this.name = name;
        }

        public Class_Function_Call(Minion minion, Expression reference = null, IEnumerable<Expression> args = null)
            : base(Expression_Type.function_call, reference, args)
        {
            this.minion = minion;
            name = minion.name;
        }

        public Class_Function_Call set_reference(Expression reference)
        {
            this.reference = reference;
            return this;
        }

        public override Profession get_profession()
        {
            return minion != null
                ? minion.return_type
                : profession;
        }

        public override Expression clone()
        {
            return new Class_Function_Call(minion, reference, args);
        }
    }

}