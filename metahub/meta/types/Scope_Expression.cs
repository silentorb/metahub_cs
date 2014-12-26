using metahub.meta.Scope;

namespace metahub.meta.types {
/**
 * @author Christopher W. Johnson
 */
public class Scope_Expression : Block
{
	public Scope scope;
	
	public Scope_Expression(Scope scope, expressions)
:base(expressions) {
		this.type = Node_Type.scope;
		this.scope = scope;
	}
}}