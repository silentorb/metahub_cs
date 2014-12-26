package metahub.imperative.types ;
using metahub.logic.schema.Tie;

/**
 * @author Christopher W. Johnson
 */

class Expression {
	public Expression_Type type;
	public Expression child = null;
	
	private Expression(Expression_Type type) {
		this.type = type;
	}
}
//struct Expression {
	//Expression_Type type,
	//Object value,
	//Expression child,
	//string name,
	//List<Object> args,
	//List<Object> path,
	//Tie tie,
	//bool is_platform_specific
//}