using metahub.logic.schema;

namespace metahub.logic.nodes {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Property_Reference : Node {
	public Tie tie;
	
	public Property_Reference(Tie tie)

:base(Node_Type.property) {
		this.tie = tie;		
	}
	
	override public Signature get_signature ()
	{
		return tie.get_signature();
	}
	
}}