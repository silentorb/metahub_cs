namespace metahub.render
{
using metahub.imperative.Imp;
using metahub.render.targets.cpp.Cpp;
using metahub.render.targets.haxe.Haxe_Target;
using metahub.render.Target;
using metahub.Hub;
using metahub.logic.schema.Railway;
using metahub.logic.schema.Region;
using metahub.meta.types.Expression;
using metahub.schema.Namespace;

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Generator {

	Hub hub;

	public Generator(Hub hub) {
		this.hub = hub;
	}
	
	public Target create_target (Imp imp, string target_name) {
		switch(target_name) {
			case "cpp":
				return new Cpp(imp.railway, imp);

			case "haxe":
				return new Haxe_Target(imp.railway, imp);
				
			default:
				throw new Exception("Unsupported target: " + target_name + ".");
		}		
	}

	public void run (Target target, string output_folder) {
		Utility.create_folder(output_folder);
		Utility.clear_folder(output_folder);
		target.run(output_folder);
	}

	public static List<string> get_namespace_path (Region region) {
		var tokens = [];
		while(region != null && region.name != "root") {
			tokens.unshift(region.external_name != null ? region.external_name : region.name);
			region = region.parent;
		}

		return tokens;
	}

}
}