using System.Collections.Generic;

namespace metahub.meta.types
{

/**
 * @author Christopher W. Johnson
 */
public class Block : Expression{
	public List<object> children = new List<object>();

	public Block(List<object> statements = null)
        : base(Expression_Type.block)
    {
		if (statements != null)
			this.children = statements;
        
	}
}
}