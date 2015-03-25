using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.schema;

namespace imperative.expressions
{
    public class Jewel : Expression
    {
        public Treasury treasury;
        public string name;

        public Jewel(Treasury treasury, string name)
            :base(Expression_Type.jewel)
        {
            this.treasury = treasury;
            this.name = name;
        }

        public override IEnumerable<Expression> children
        {
            get { return new List<Expression>(); }
        }
    }
}
