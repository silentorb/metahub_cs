using metahub.logic.schema.Signature;
using metahub.schema.Kind;

namespace s {
/**
 * @author Christopher W. Johnson
 */

class Array_Expression extends Expression{
	public List<Expression> children;

	public Array_Expression(List<Expression> children = null) {
		this.children = children != null
		 ? children : [];
		
		super(Expression_Type.array);
	}
	
	override public Signature get_signature ()
	{
		return { type: Kind.list, rail: null };
	}
}}