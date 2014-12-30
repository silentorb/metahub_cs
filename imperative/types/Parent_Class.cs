using metahub.logic.types;

namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Parent_Class : Expression
{
	public Parent_Class(Expression child)

        : base(Expression_Type.parent_class)
    {
		this.child = child;
	}
	
}
}