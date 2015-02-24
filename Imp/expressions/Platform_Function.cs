using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.schema;

namespace metahub.imperative.expressions
{
    public class Platform_Function : Function_Call
    {
        public string name;
        public Profession profession;
        public Platform_Function_Info info;

        public Platform_Function(string name, Expression reference = null, IEnumerable<Expression> args = null)
            : base(Expression_Type.platform_function, reference, args)
        {
            this.name = name;
            info = Platform_Function_Info.functions[name];
            profession = info.return_type.clone();
            if (profession.type == Kind.reference)
            {
                if (reference == null)
                    throw new Exception("A platform function that returns a reference must have a calling reference.");

                profession.dungeon = reference.get_profession().dungeon;
            }
        }

        public override Profession get_profession()
        {
            return profession;
        }
    }

}