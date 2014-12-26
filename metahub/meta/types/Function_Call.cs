using System;
using metahub.logic.schema;
using metahub.logic.schema.Railway;
using metahub.logic.schema.Signature;

namespace metahub.meta.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Function_Call : Expression {
	public string name;
	public Expression input;
	public Signature signature;

	public Function_Call(string name, Expression input, Railway railway)
        : base(Expression_Type.function_call)
    {
		;
		this.name = name;
		if (input == null)
			throw new Exception("Function input cannot be null");
			
		this.input = input;
		signature = railway.root_region.functions[name].get_signature(this);
	}

	override public Signature get_signature ()
	{
		return signature;
	}
}}