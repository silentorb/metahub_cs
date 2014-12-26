package metahub.imperative.types ;

/**
 * @author Christopher W. Johnson
 */

class Function_Call extends Expression {
	public string name;
	public List<Object> args;
	public bool is_platform_specific;
	
	public Function_Call(string name, List<Object> args = null, bool is_platform_specific = false) {
		super(Expression_Type.function_call);
		this.name = name;
		this.is_platform_specific = is_platform_specific;
		this.args = args != null ? args : [];
	}
}
//struct Function_Call {
		//string type,
		//string name,
		//string caller,
		//List<Object> args,
		//bool is_platform_specific
//}