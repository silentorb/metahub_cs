using metahub.code.functions.Function;
using metahub.code.functions.Function_Library;
using metahub.code.nodes.Group;
using metahub.Hub;
using metahub.schema.Namespace;
using metahub.schema.Kind;
using metahub.code.Type_Signature;

namespace h {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Math_Library implements Function_Library
{
	public string name;
	public Dictionary<string, int> function_map = new Dictionary<string, int>();
	public Map <int, Array < List<Type_Signature>>>  signatures = new Map <int, Array < List<Type_Signature>>> ();
	Hub hub;

	public Math_Library()
	{
		name = "Math";
	}

	public void load (Hub hub) {
		this.hub = hub;
		Type_Signature type_float = new Type_Signature(Kind.Float);

		function_map["random"] = 0;
		signatures[0] = [
			[ type_float ]
		];
	}

	public bool exists (string function_string) {
		return function_map.ContainsKey(function_string);
	}

	public int get_function_id (string function_string) {
		return function_map[function_string];
	}

	public void create_node (int func, List<Type_Signature> signature, Group group, bool is_constraint) {
		return new Math_Functions(hub, func, signature, group, is_constraint);
	}

	public List<Array get_function_options (int func) < Type_Signature >> {
		if (!signatures.ContainsKey(func))
			throw new Exception("Function " + func + " is not yet implemented.");

		return signatures[func];
	}

}}