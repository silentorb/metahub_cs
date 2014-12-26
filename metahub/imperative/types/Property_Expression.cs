using metahub.logic.schema.Tie;

namespace s {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Property_Expression extends Expression
{
	public Tie tie;
	
	public Property_Expression(Tie tie, Expression child = null)
	{
		super(Expression_Type.property);
		this.tie = tie;
		this.child = child;
	}
	
}}