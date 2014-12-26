using metahub.logic.schema;
using metahub.meta.types;

namespace metahub.imperative.types
{
    public class Instantiate : Expression
    {
        public Rail rail;

        public Instantiate(Rail rail)
            : base(Node_Type.instantiate)
        {
            this.rail = rail;
        }
    }

}