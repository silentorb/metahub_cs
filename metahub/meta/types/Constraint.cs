namespace metahub.meta.types
{

/**
 * @author Christopher W. Johnson
 */
public class Constraint : Expression
{
	public Expression first;
	public Expression second;
	public string op;
	public Lambda lambda;
	
	public Constraint(Expression first, Expression second, string op = "=", Lambda lambda = null)
        : base(Expression_Type.constraint)
    {
		this.first = first;
		this.second = second;
		this.op = op;
		this.lambda = lambda;
	}
}
}