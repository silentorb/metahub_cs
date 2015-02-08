using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.nodes;
using metahub.schema;

namespace metahub.imperative.types
{
    public class Instantiate : Expression
    {
        public Rail rail;
        public Dungeon dungeon;

        public Instantiate(Rail rail)
            : base(Expression_Type.instantiate)
        {
            this.rail = rail;
        }

        public Instantiate(Dungeon dungeon)
            : base(Expression_Type.instantiate)
        {
            this.dungeon = dungeon;
        }

        public override Profession get_profession()
        {
            return new Profession(Kind.reference, dungeon);
        }
    }

}