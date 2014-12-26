namespace metahub.imperative.types
{
using metahub.logic.schema.Rail;

/**
 * @author Christopher W. Johnson
 */
public class Instantiate : Expression {
	public Rail rail;
	
	public Instantiate(Rail rail)
:base(Expression_Type.instantiate) {
		this.rail = rail;
	}
}

//struct Instantiate //{
	//string type,
	//Rail rail
//}
}