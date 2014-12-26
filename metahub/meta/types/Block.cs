package metahub.meta.types;

/**
 * @author Christopher W. Johnson
 */

class Block extends Expression{
	public List<Object> children = new List<Object>();

	public Block(List<Object> statements = null) {
		if (statements != null)
			this.children = statements;

		super(Expression_Type.block);
	}
}