using metahub.logic.schema;
using metahub.logic.schema.Signature;
using metahub.meta.types;

namespace metahub.imperative.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Literal : Expression {
	public object value;
	public Signature signature;

	public Literal(object value, Signature signature)

:base(Expression_Type.literal) {
		this.value = value;
		this.signature = signature;
	}
	
}}