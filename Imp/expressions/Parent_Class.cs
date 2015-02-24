
namespace metahub.imperative.expressions
{
    public class Parent_Class : Expression
    {
        public Parent_Class(Expression child = null)

            : base(Expression_Type.parent_class, child)
        {
        }

    }
}