using System.Collections.Generic;
using System.Linq;

namespace metahub.imperative.expressions
{
    public class Create_Array : Expression
    {
        public List<Expression> items;

        public Create_Array(IEnumerable<Expression> children)
            : base(Expression_Type.create_array)
        {
            this.items = children.ToList();
        }

        public override IEnumerable<Expression> children
        {
            get { return items; }
        }
    }
}