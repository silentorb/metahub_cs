using metahub.logic.schema;
using metahub.meta.types;

namespace metahub.imperative.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Property_Expression : Expression
{
	public Tie tie;
	
	public Property_Expression(Tie tie, Expression child = null)

:base(Expression_Type.property) {
		this.tie = tie;
		this.child = child;
	}
	
}}