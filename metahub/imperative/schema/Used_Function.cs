package metahub.imperative.schema;

/**
 * ...
 * @author Christopher W. Johnson
 */

class Used_Function {
	public string name;
	public bool is_platform_specific;
	
	public Used_(string name, bool is_platform_specific = false)=>{
		this.name = name;
		this.is_platform_specific = is_platform_specific;
	}
}