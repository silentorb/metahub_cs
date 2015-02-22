

namespace metahub.imperative.types
{
    public class Statement : Expression
    {
        public string name;

        public Statement(string name, Expression child = null)
            : base(Expression_Type.statement, child)
        {
            this.name = name;
        }

    }

}