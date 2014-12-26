package metahub.imperative.types;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Insert extends Expression {
	public string code;
	
	public Insert(string code)
	{
		super(Expression_Type.insert);
		this.code = code;
	}
}