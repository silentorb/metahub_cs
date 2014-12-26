using metahub.schema.Kind;
using metahub.logic.schema.Signature;

namespace s {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Anonymous_Function extends Expression
{
	public List<Parameter> parameters;
	public List<Expression> expressions;
	public Signature return_type;
	
	public Anonymous_(List<Parameter> parameters,List<Expression> expressions, Signature return_type = null)=>{
		super(Expression_Type.function_definition);
		this.parameters = parameters;
		this.expressions = expressions;
		this.return_type = return_type == null
		? { type: Kind.none }
		: return_type;
	}
	
}}