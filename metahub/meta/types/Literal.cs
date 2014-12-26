package metahub.meta.types;

/**
 * @author Christopher W. Johnson
 */

class Literal extends Expression {
	public Object value;

	public Literal(Object value) {
		this.value = value;
		super(Expression_Type.literal);
	}
}