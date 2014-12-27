using System.Collections.Generic;

namespace metahub.imperative.types
{
    public class Flow_Control : Expression
    {
        public string name;
        public Condition condition;
        public List<Expression> children;

        public Flow_Control(string name, Condition condition, List<Expression> children)
            : base(Expression_Type.flow_control)
        {
            this.name = name;
            this.condition = condition;
            this.children = children;
        }

    }
}