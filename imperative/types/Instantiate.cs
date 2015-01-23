using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.nodes;

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
    }

}