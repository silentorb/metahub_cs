package metahub.imperative.types ;

/**
 * ...
 * @author Christopher W. Johnson
 */

class Statement extends Expression {
	public string name;

	public Statement(string name)
	{
		super(Expression_Type.statement);
		this.name = name;
	}
	
}
//struct Statement {
	//Expression_Type type
//}