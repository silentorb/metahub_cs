using metahub.meta.Scope;

namespace metahub.meta.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Lambda : Node
{
	public List<Parameter> parameters;
	public List<Node> expressions;
	public Scope scope;

	public Lambda(Scope scope, List<Parameter> parameters, List<Node> expressions)

:base(Node_Type.lambda) {
		this.scope = scope;
		this.parameters = parameters;
		this.expressions = expressions;		
	}
	
}}