using metahub.meta.Scope;

namespace metahub.meta.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Lambda : Expression
{
	public List<Parameter> parameters;
	public List<Expression> expressions;
	public Scope scope;

	public Lambda(Scope scope, List<Parameter> parameters, List<Expression> expressions)

:base(Expression_Type.lambda) {
		this.scope = scope;
		this.parameters = parameters;
		this.expressions = expressions;		
	}
	
}}