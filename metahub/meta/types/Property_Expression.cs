using metahub.logic.schema.Signature;
using metahub.logic.schema.Tie;

namespace metahub.meta.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Property_Expression : Expression {
	public Tie tie;
	
	public Property_Expression(Tie tie)

:base(Expression_Type.property) {
		this.tie = tie;		
	}
	
	override public Signature get_signature ()
	{
		return tie.get_signature();
	}
	
}}