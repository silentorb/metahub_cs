using metahub.meta.types;

namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Statement : Expression {
	public string name;

	public Statement(string name)

:base(Node_Type.statement) {
		this.name = name;
	}
	
}
//struct Statement {
	//Expression_Type type
//}
}