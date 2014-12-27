using metahub.meta.types;

namespace metahub.imperative.types
{
    public class Statement : Expression
    {
        public string name;

        public Statement(string name)
            : base(Expression_Type.statement)
        {
            this.name = name;
        }

    }

}