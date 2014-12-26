using metahub.meta.Scope;

namespace s {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Lambda extends Expression
{
	public List<Parameter> parameters;
	public List<Expression> expressions;
	public Scope scope;

	public Lambda(Scope scope, List<Parameter> parameters, List<Expression> expressions)
	{
		super(Expression_Type.lambda);
		this.scope = scope;
		this.parameters = parameters;
		this.expressions = expressions;		
	}
	
}}