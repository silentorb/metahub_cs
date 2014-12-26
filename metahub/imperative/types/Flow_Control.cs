package metahub.imperative.types ;

/**
 * @author Christopher W. Johnson
 */

 
class Flow_Control extends Expression {
	public string name;
	public Condition condition;
	public List<Expression> children;
	
	public Flow_Control(string name, Condition condition, List<Expression> children) {
		super(Expression_Type.flow_control);
		this.name = name;
		this.condition = condition;
		this.children = children;
	}
	
}
//struct Flow_Control {
	//Expression_Type type,
	//string name,
	//Condition condition,
	//List<Object> statements,
//}