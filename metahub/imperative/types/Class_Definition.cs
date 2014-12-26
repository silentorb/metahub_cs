package metahub.imperative.types ;
using metahub.logic.schema.Rail;

/**
 * @author Christopher W. Johnson
 */

class Class_Definition extends Expression {
	public Rail rail;
	public List<Expression> expressions;
	
	public Class_Definition(Rail rail, List<Expression> statements)
	{
		super(Expression_Type.class_definition);
		this.rail = rail;
		this.expressions = statements;
	}
}