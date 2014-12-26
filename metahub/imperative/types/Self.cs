package metahub.imperative.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Self extends Expression {

	public Self(Expression child = null)
	{
		super(Expression_Type.self);
		this.child = child;
	}
	
}