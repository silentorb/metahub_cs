using metahub.meta.types;

namespace metahub.imperative.types
{

/**
 * @author Christopher W. Johnson
 */
public class Expression {
	public Node_Type type;
	public Expression child = null;
	
	protected Expression(Node_Type type) {
		this.type = type;
	}
}
//struct Node {
	//Expression_Type type,
	//object value,
	//Node child,
	//string name,
	//List<object> args,
	//List<object> path,
	//Tie tie,
	//bool is_platform_specific
//}
}