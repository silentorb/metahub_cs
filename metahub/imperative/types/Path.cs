package metahub.imperative.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Path extends Expression {
	public List<Expression> children;
	
	public Path(List<Expression> children)
	{
		super(Expression_Type.path);
		this.children = children;
	}
	
}