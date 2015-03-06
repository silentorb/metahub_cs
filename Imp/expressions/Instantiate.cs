using System.Collections;
using System.Collections.Generic;
using System.Linq;
using imperative.schema;


using metahub.schema;

namespace imperative.expressions
{
    public class Instantiate : Expression
    {
        public Dungeon dungeon;
        public List<Expression> args;

        public Instantiate(Dungeon dungeon, IEnumerable<Expression> args = null)
            : base(Expression_Type.instantiate)
        {
            this.dungeon = dungeon;
            this.args = args != null ? args.ToList() : new List<Expression>();
        }

        public override Profession get_profession()
        {
            return new Profession(Kind.reference, dungeon);
        }

        public override IEnumerable<Expression> children
        {
            get { return args; }
        }
    }

}