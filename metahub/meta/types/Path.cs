using metahub.logic.schema.Signature;

namespace metahub.meta.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Path : Expression {
	public List<Expression> children;
	
	public Path(List<Expression> children)

:base(Expression_Type.path) {
		this.children = children;		
	}
	
	override public Signature get_signature ()
	{
		if (children.Count() == 0)
			throw new Exception("Cannot find signature of empty array.");
		
		return children[children.Count() - 1].get_signature();
	}
}}