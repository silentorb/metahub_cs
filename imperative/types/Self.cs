using metahub.imperative.schema;
using metahub.logic.types;
using metahub.schema;

namespace metahub.imperative.types
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
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
    }
}