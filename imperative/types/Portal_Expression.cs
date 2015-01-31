using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.schema;
using metahub.logic.schema;

namespace metahub.imperative.types
{
    [DebuggerDisplay("portal ref ({portal.fullname})")]
    public class Portal_Expression : Expression
    {
        public Portal portal;
        public Expression index;

        public Portal_Expression(Portal portal, Expression child = null)

            : base(Expression_Type.portal, child)
        {
            if (portal == null)
                throw new Exception("portal cannot be null.");

            this.portal = portal;
        }

        public override Signature get_signature()
        {
            return index == null
                ? portal.get_signature()
                : new Signature(Kind.reference, portal.other_rail);
        }

        public override Expression clone()
        {
            return new Portal_Expression(portal, child != null ? child.clone() : null)
                {
                    index = index != null ? index.clone() : null
                };
        }

        public override Profession get_profession()
        {
            return portal.get_profession();
        }
    }
}
