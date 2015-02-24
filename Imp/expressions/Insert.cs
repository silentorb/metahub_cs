namespace metahub.imperative.expressions
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Insert : Expression {
	public string code;
	
	public Insert(string code)

:base(Expression_Type.insert) {
		this.code = code;
	}
}
}