package metahub.imperative.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Parent_Class extends Expression
{
	public Parent_Class(Expression child)
	{
		super(Expression_Type.parent_class);
		this.child = child;
	}
	
}