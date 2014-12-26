package metahub.imperative.types ;
using metahub.logic.schema.Rail;

/**
 * @author Christopher W. Johnson
 */

 class Instantiate extends Expression {
	public Rail rail;
	
	public Instantiate(Rail rail) {
		super(Expression_Type.instantiate);
		this.rail = rail;
	}
}

//struct Instantiate //{
	//string type,
	//Rail rail
//}