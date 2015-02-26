using System.Collections.Generic;
using metahub.imperative.schema;


namespace metahub.imperative.expressions
{

    public class Class_Definition : Block
    {
        public Dungeon dungeon;

        public Class_Definition(Dungeon dungeon, List<Expression> statements)

            : base(Expression_Type.class_definition, statements)
        {
            this.dungeon = dungeon;
        }
    }
}