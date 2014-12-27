using System.Collections.Generic;

namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Path : Expression {
	public List<Expression> children;
	
	public Path(List<Expression> children)

:base(Expression_Type.path) {
		this.children = children;
	}
	
}
}