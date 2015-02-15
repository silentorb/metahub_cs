using metahub.logic.nodes;

namespace metahub.imperative.types
{
    public class Null_Value : Expression
    {
        public Null_Value()

            : base(Expression_Type.null_value)
        {
        }

        public override Expression clone()
        {
            return new Null_Value();
        }
    }
}