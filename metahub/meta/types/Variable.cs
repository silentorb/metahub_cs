namespace metahub.meta.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Variable : Node
{
	public string name;
	
	public Variable(string name)
:base(Node_Type.variable) {
		this.name = name;
	}
	
}
}