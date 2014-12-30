using metahub.logic.schema;
using metahub.logic.types;

namespace metahub.imperative.types
{
    public class Declare_Variable : Expression
    {
        public string name;
        public Signature signature;
        public Expression expression;

        public Declare_Variable(string name, Signature signature, Expression expression)
            : base(Expression_Type.declare_variable)
        {
            this.name = name;
            this.signature = signature;
            this.expression = expression;
        }
    }

}