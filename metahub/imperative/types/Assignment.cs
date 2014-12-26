namespace metahub.imperative.types
{

/**
 * @author Christopher W. Johnson
 */
public class Assignment : Expression {
	public string op;
	public Expression target;
	public Expression expression;
	
	public Assignment(Expression target, string op, Expression expression)
:base(Expression_Type.assignment) {
		this.op = op;
		this.target = target;
		this.expression = expression;
	}
}
//struct Assignment //{
	//string type,
	//string op,
	//Expression target,
	//Expression expression
//}
}