package metahub.imperative.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Create_Array extends Expression
{
	public List<Expression> children;
	
	public Create_Array(List<Expression> children) {
		super(Expression_Type.create_array);
		this.children = children;
	}	
}