package metahub.imperative.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Variable extends Expression {
	public string name;

	public Variable(string name, Expression child = null)
	{
		super(Expression_Type.variable);
		this.name = name;
		this.child = child;
	}
	
}