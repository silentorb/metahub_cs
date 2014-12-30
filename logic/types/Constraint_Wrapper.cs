using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;

namespace metahub.logic.types
{
    public class Constraint_Wrapper : Node
    {
        public Constraint constraint;

        public Constraint_Wrapper(Constraint constraint)
            : base(Node_Type.constraint)
        {
            this.constraint = constraint;

        }
    }
}