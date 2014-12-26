package metahub.meta.types;

/**
 * @author Christopher W. Johnson
 */

class Expression {
	public Expression_Type type;
	
	Expression(Expression_Type type) {
		this.type = type;
	}
	
	public metahub.logic.schema.Signature get_signature () {
		throw Type.getClassName(Type.getClass(this)) + " does not implement get_signature().";
	}
}