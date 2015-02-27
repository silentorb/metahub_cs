using System.Collections.Generic;
using imperative.schema;


using metahub.schema;

namespace imperative.expressions
{
    public class Instantiate : Expression
    {
        public Dungeon dungeon;

        public Instantiate(Dungeon dungeon)
            : base(Expression_Type.instantiate)
        {
            this.dungeon = dungeon;
        }

        public override Profession get_profession()
        {
            return new Profession(Kind.reference, dungeon);
        }

        public override IEnumerable<Expression> children
        {
            get { return new List<Expression>(); }
        }
    }

}