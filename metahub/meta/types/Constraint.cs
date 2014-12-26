package metahub.meta.types;

/**
 * @author Christopher W. Johnson
 */

class Constraint extends Expression
{
	public Expression first;
	public Expression second;
	public string operator;
	public Lambda lambda;
	
	public Constraint(first, second, operator = "=", Lambda lambda = null) {
		super(Expression_Type.constraint);
		this.first = first;
		this.second = second;
		this.operator = operator;
		this.lambda = lambda;
	}
}