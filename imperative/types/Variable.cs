using metahub.logic.types;

namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Variable : Expression {
	public string name;

	public Variable(string name, Expression child = null)

        : base(Expression_Type.variable)
    {
		this.name = name;
		this.child = child;
	}
	
}
}