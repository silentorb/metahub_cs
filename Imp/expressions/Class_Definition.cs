using System.Collections.Generic;
using metahub.imperative.schema;


namespace metahub.imperative.expressions
{

/**
 * @author Christopher W. Johnson
 */
public class Class_Definition : Expression {
	public Dungeon dungeon;
	public List<Expression> expressions;

    public Class_Definition(Dungeon dungeon, List<Expression> statements)

        : base(Expression_Type.class_definition)
    {
		this.dungeon = dungeon;
		this.expressions = statements;
	}
}
}