using metahub.logic.nodes;

namespace metahub.imperative.types
{
    public class Parent_Class : Expression
    {
        public Parent_Class(Expression child = null)

            : base(Expression_Type.parent_class, child)
        {
        }

    }
}