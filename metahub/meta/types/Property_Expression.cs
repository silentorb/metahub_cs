using metahub.logic.schema.Signature;
using metahub.logic.schema.Tie;

namespace s {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Property_Expression extends Expression {
	public Tie tie;
	
	public Property_Expression(Tie tie)
	{
		super(Expression_Type.property);
		this.tie = tie;		
	}
	
	override public Signature get_signature ()
	{
		return tie.get_signature();
	}
	
}}