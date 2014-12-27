using System.Collections.Generic;
using metahub.logic.schema;
using metahub.meta.types;
using metahub.schema;

namespace metahub.imperative.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Anonymous_Function : Expression
{
	public List<Parameter> parameters;
	public List<Expression> expressions;
	public Signature return_type;
	
	public Anonymous_Function(List<Parameter> parameters,List<Expression> expressions, Signature return_type = null)
        : base(Expression_Type.function_definition)
    {
		this.parameters = parameters;
		this.expressions = expressions;
		this.return_type = return_type ?? new Signature(Kind.none);
	}
	
}}