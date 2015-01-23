using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;

namespace metahub.imperative.types
{
    public class Platform_Function : Function_Call
    {
        public string name;
        public Profession profession;

        public Platform_Function(string name, Expression reference = null, IEnumerable<Expression> args = null)
            : base(Expression_Type.platform_function, reference, args)
        {
            this.name = name;
        }

        public override Profession get_profession()
        {
            return profession;
        }
    }

}