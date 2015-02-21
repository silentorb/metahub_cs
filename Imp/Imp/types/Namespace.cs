using System.Collections.Generic;
using metahub.imperative.schema;

namespace metahub.imperative.types
{
    public class Namespace : Expression
    {
        public Realm realm;
        public List<Expression> expressions;

        public Namespace(Realm realm, List<Expression> block)
            : base(Expression_Type.space)
        {
            this.realm = realm;
            this.expressions = block;
        }
    }
}