using metahub.logic.schema.Signature;

namespace s {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Literal extends Expression {
	public Object value;
	public Signature signature;

	public Literal(Object value, Signature signature)
	{
		super(Expression_Type.literal);
		this.value = value;
		this.signature = signature;
	}
	
}}