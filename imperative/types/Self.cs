using metahub.logic.types;

namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Self : Expression {

	public Self(Expression child = null)

:base(Expression_Type.self) {
		this.child = child;
	}
	
}
}