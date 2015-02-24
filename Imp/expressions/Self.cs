using metahub.imperative.schema;
using metahub.schema;

namespace metahub.imperative.expressions
{
    public class Self : Expression
    {
        public Dungeon dungeon;

        public Self(Dungeon dungeon, Expression child = null)
            : base(Expression_Type.self, child)
        {
            this.dungeon = dungeon;
        }

        public override Profession get_profession()
        {
            return new Profession(Kind.reference, dungeon);
        }

        public override Expression clone()
        {
            return new Self(dungeon, child != null ? child.clone() : null);
        }

        public override bool is_token { get { return true; } }
    }
}