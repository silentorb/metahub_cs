using metahub.meta.Scope;

namespace s {
/**
 * @author Christopher W. Johnson
 */

class Scope_Expression extends Block
{
	public Scope scope;
	
	public Scope_Expression(Scope scope, expressions) {
		super(expressions);
		this.type = Expression_Type.scope;
		this.scope = scope;
	}
}}