package metahub.meta.types;

/**
 * ...
 * @author Christopher W. Johnson
 */

class Function_Scope extends Expression
{
	public Expression expression;
	public Lambda lambda;
	
	public Function_Scope(Expression expression, Lambda lambda)
	{
		super(Expression_Type.function_scope);
		this.expression = expression;
		this.lambda = lambda;
	}
	
}