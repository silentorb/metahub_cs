using metahub.imperative.schema;
using metahub.logic.nodes;
using metahub.schema;

namespace metahub.imperative.types
{
    public class Self : Expression
    {
        public Dungeon dungeon;

        public Self(Dungeon dungeon, Expression child = null)
            : base(Expression_Type.self, child)
        {
            this.dungeon = dungeon;
        }

        public override logic.schema.Signature get_signature()
        {
            return new logic.schema.Signature(Kind.reference, dungeon.rail);
        }

        public override Profession get_profession()
        {
            return new Profession(Kind.reference, dungeon);
        }

        public override Expression clone()
        {
            return new Self(dungeon, child != null ? child.clone() : null);
        }
    }
}