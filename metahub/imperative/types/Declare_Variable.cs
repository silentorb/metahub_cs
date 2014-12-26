package metahub.imperative.types ;
using metahub.logic.schema.Signature;

/**
 * @author Christopher W. Johnson
 */

class Declare_Variable extends Expression {
	public string name;
	public Signature signature;
	public Expression expression;
	
	public Declare_Variable(string name, Signature signature, Expression expression) {
		super(Expression_Type.declare_variable);
		this.name = name;
		this.signature = signature;
		this.expression = expression;
	}
}
//struct Declare_Variable //{
	//string type,
	//string name,
	//Signature signature,
	//Expression expression
//}