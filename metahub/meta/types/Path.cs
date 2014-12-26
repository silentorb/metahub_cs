using metahub.logic.schema.Signature;

namespace s {
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
	
	override public Signature get_signature ()
	{
		if (children.Count() == 0)
			throw new Exception("Cannot find signature of empty array.");
		
		return children[children.Count() - 1].get_signature();
	}
}}