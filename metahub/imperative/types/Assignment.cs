package metahub.imperative.types ;

/**
 * @author Christopher W. Johnson
 */

class Assignment extends Expression {
	public string operator;
	public Expression target;
	public Expression expression;
	
	public Assignment(Expression target, string operator, Expression expression) {
		super(Expression_Type.assignment);
		this.operator = operator;
		this.target = target;
		this.expression = expression;
	}
}
//struct Assignment //{
	//string type,
	//string operator,
	//Expression target,
	//Expression expression
//}