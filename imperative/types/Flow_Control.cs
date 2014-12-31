using System.Collections.Generic;

namespace metahub.imperative.types
{
    public class Flow_Control : Expression
    {
        public string name;
        public Expression expression;
        public List<Expression> children;

        public Flow_Control(string name, Expression expression, List<Expression> children)
            : base(Expression_Type.flow_control)
        {
            this.name = name;
            this.expression = expression;
            this.children = children;
        }

    }
}