using System;
using metahub.logic.schema;

namespace metahub.meta.types {

public class Function_Call : Node {
	public string name;
	public Node input;
	public Signature signature;

	public Function_Call(string name, Node input, Railway railway)
        : base(Node_Type.function_call)
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