using metahub.logic.schema;
using metahub.logic.types;

namespace metahub.imperative.types
{
    public class Instantiate : Expression
    {
        public Rail rail;

        public Instantiate(Rail rail)
            : base(Expression_Type.instantiate)
        {
            this.rail = rail;
        }
    }

}