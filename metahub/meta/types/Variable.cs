namespace metahub.meta.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Variable : Expression
{
	public string name;
	
	public Variable(string name)
:base(Expression_Type.variable) {
		this.name = name;
	}
	
}
}