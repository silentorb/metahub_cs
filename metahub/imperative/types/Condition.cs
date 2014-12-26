package metahub.imperative.types ;

/**
 * @author Christopher W. Johnson
 */

class Condition {
	public string operator;
	public Object expressions;
	
	public Condition(string operator, Object expressions) {
		this.operator = operator;
		this.expressions = expressions;
		if (expressions[0].type == 7)
		throw "";
	}
}