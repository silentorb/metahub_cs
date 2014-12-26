namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Create_Array : Expression
{
	public List<Expression> children;
	
	public Create_Array(List<Expression> children)
:base(Expression_Type.create_array) {
		this.children = children;
	}	
}
}