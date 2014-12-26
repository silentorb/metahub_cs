using System.Collections.Generic;
using metahub.meta.types;

namespace metahub.imperative.types
{

/**
 * @author Christopher W. Johnson
 */
public class Flow_Control : Expression {
	public string name;
	public Condition condition;
	public List<Expression> children;
	
	public Flow_Control(string name, Condition condition, List<Expression> children)
:base(Node_Type.flow_control) {
		this.name = name;
		this.condition = condition;
		this.children = children;
	}
	
}
//struct Flow_Control {
	//Expression_Type type,
	//string name,
	//Condition condition,
	//List<object> statements,
//}
}