using metahub.imperative.schema;
using metahub.logic.types;
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
    }
}