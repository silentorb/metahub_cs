package metahub.logic.schema ;
using metahub.schema.Namespace;

/**
 * ...
 * @author Christopher W. Johnson
 */

struct Region_Additional {
	bool is_external,
	string namespace,
	string class_export
}
 
class Region
{
	public Namespace namespace;
	public bool is_external = false;
	public Dictionary<string,Object> trellis_additional = new Dictionary<string,Object>();
	public string external_name = null;
	public Dictionary<string, Rail> rails = new Dictionary<string, Rail>();
	public string class_export = "";
	public string name;
	public Region parent = null;
	public Dictionary<string, Region> children = new Dictionary<string, Region>();
	public Dictionary<string, Function_Info> functions = new Dictionary<string, Function_Info>();

	public Region(Namespace namespace, string target_name)
	{
		this.space = namespace;
		name = space.name;
		is_external = space.is_external;
		
		if (space.additional == null)
			return;
			
		Region_Additional additional = space.additional[target_name];
		if (additional == null)
			return;
		
		if (additional.ContainsKey("is_external"))
			is_external = additional.is_external;

		if (additional.ContainsKey("namespace"))
			external_name = additional.space;

		if (additional.ContainsKey("class_export"))
			class_export = additional.class_export;
				
		var trellises = additional["trellises"];
		if (trellises != null) {
			foreach (var key in Reflect.fields(trellises)) {
				trellis_additional[key] = trellises[key];
			}
		}
	}
	
	public void add_functions (List<Function_Info> new_functions ) {
		foreach (var func in new_functions) {
			functions[func.name] = func;
		}
	}
	
}