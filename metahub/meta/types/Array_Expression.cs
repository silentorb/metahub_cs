using metahub.logic.schema.Signature;
using metahub.schema.Kind;

namespace metahub.meta.types {
/**
 * @author Christopher W. Johnson
 */
public class Array_Expression : Expression{
	public List<Expression> children;

	public Array_Expression(List<Expression> children = null) {
		this.children = children != null
		 ? children : [];
		
		base(Expression_Type.array);
	}
	
	override public Signature get_signature ()
	{
		return { type: Kind.list, rail: null };
	}
}}