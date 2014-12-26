using System.Collections.Generic;
using metahub.logic.schema;
using metahub.meta.types;

namespace metahub.imperative.types
{
using metahub.logic.schema.Rail;

/**
 * @author Christopher W. Johnson
 */
public class Class_Definition : Expression {
	public Rail rail;
	public List<Expression> expressions;
	
	public Class_Definition(Rail rail, List<Expression> statements)

:base(Expression_Type.class_definition) {
		this.rail = rail;
		this.expressions = statements;
	}
}
}