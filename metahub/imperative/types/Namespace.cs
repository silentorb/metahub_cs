package metahub.imperative.types ;
using metahub.logic.schema.Region;

/**
 * @author Christopher W. Johnson
 */

class Namespace extends Expression {
	public Region region;
	public List<Expression> expressions;
	
	public Namespace(Region region, List<Expression> block)
	{
		super(Expression_Type.space);
		this.region = region;
		this.expressions = block;
	}
}