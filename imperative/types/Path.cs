using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;

namespace metahub.imperative.types
{
    public class Path : Expression
    {
        public List<Expression> children;

        public Path(IEnumerable<Expression> children)

            : base(Expression_Type.path)
        {
            this.children = children.ToList();
        }

        public override Profession get_profession()
        {
            return children.Last().get_profession();
        }

        public override logic.schema.Signature get_signature()
        {
            return children.Last().get_signature();
        }
    }
}