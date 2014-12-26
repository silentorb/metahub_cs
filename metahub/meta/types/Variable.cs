package metahub.meta.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Variable extends Expression
{
	public string name;
	
	public Variable(string name) {
		super(Expression_Type.variable);
		this.name = name;
	}
	
}