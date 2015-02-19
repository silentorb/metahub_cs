using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.nodes;
using metahub.schema;

namespace metahub.imperative.types {

public class Anonymous_Function : Expression
{
	public List<Parameter> parameters;
	public Profession _return_type;

    public Anonymous_Function(List<Parameter> parameters, List<Expression> expressions, Profession return_type = null)
        : base(Expression_Type.function_definition)
    {
		this.parameters = parameters;
		_return_type = return_type ?? new Profession(Kind.none);
	}
	
}}