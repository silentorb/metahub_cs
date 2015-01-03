using System.Collections.Generic;

namespace metahub.imperative.types
{
    public enum Flow_Control_Type
    {
        If,
        While
    }

    public class Flow_Control : Expression
    {
        public Flow_Control_Type flow_type;
        public Expression expression;
        public List<Expression> children;

        public Flow_Control(Flow_Control_Type flow_type, Expression expression, List<Expression> children)
            : base(Expression_Type.flow_control)
        {
            this.flow_type = flow_type;
            this.expression = expression;
            this.children = children;
        }

    }
}