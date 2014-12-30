using System.Collections.Generic;
using System.Linq;

namespace metahub.imperative.types
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Path : Expression {
	public List<Expression> children;
	
	public Path(IEnumerable<Expression> children)

:base(Expression_Type.path) {
		this.children = children.ToList();
	}
	
}
}