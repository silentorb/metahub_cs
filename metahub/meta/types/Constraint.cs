namespace metahub.meta.types
{

/**
 * @author Christopher W. Johnson
 */
public class Constraint : Node
{
	public Node first;
	public Node second;
	public string op;
	public Lambda lambda;
	
	public Constraint(Node first, Node second, string op = "=", Lambda lambda = null)
        : base(Node_Type.constraint)
    {
		this.first = first;
		this.second = second;
		this.op = op;
		this.lambda = lambda;
	}
}
}