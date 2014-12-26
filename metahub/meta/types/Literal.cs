namespace metahub.meta.types
{

/**
 * @author Christopher W. Johnson
 */
public class Literal : Expression {
	public object value;

	public Literal(object value) {
		this.value = value;
		base(Expression_Type.literal);
	}
}
}