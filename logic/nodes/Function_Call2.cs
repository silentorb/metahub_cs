using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;

namespace metahub.logic.nodes {

public class Function_Call2 : Node {
	public string name;

	public Function_Call2(string name, IEnumerable<Node> inputs)
        : base(Node_Type.function_call)
    {
		this.name = name;
		if (inputs == null)
			throw new Exception("Function input cannot be null");
			
	    foreach (var input in inputs)
	    {
	        connect_input(input);
	    }
    }
}}