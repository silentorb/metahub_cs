using System.Collections.Generic;
using metahub.logic.schema;

namespace metahub.imperative.types
{
using metahub.logic.schema.Region;

/**
 * @author Christopher W. Johnson
 */
public class Namespace : Expression {
	public Region region;
	public List<Expression> expressions;
	
	public Namespace(Region region, List<Expression> block)

:base(Expression_Type.space) {
		this.region = region;
		this.expressions = block;
	}
}
}