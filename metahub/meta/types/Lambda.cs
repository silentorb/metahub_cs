using System.Collections.Generic;

namespace metahub.meta.types {

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