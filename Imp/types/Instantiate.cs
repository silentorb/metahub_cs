using metahub.imperative.schema;


using metahub.schema;

namespace metahub.imperative.types
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
    }

}