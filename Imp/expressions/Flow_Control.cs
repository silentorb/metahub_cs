using System.Collections.Generic;
using System.Linq;

namespace metahub.imperative.expressions
{
    public enum Flow_Control_Type
    {
        If,
        While
    }

    public class Flow_Control : Expression
    {
        public Flow_Control_Type flow_type;
        public Expression condition
        {
            get { return children[0]; }   
        }
        public List<Expression> body
        {
            get { return children.Skip(1).ToList(); }
        }

        public Flow_Control(Flow_Control_Type flow_type, Expression condition, IEnumerable<Expression> body)
            : base(Expression_Type.flow_control)
        {
            this.flow_type = flow_type;
            children = new[] { condition }.Concat(body).ToList();
        }

    }
}