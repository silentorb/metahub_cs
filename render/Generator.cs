using System;
using System.Collections.Generic;
using metahub.imperative;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.render.targets.cpp;
using metahub.render.targets.js;
using metahub.render.targets.php;

namespace metahub.render
{

public class Generator {

	Hub hub;

	public Generator(Hub hub) {
		this.hub = hub;
	}
	
	public Target create_target (Overlord imp, string target_name) {
		switch(target_name) {
			case "cpp":
				return new Cpp(imp.railway, imp);

			case "js":
				return new Js_Target(imp.railway, imp);

            case "php":
                return new Php_Target(imp.railway, imp);
				
			default:
				throw new Exception("Unsupported target: " + target_name + ".");
		}		
	}

	public void run (Target target, string output_folder) {
		Utility.create_folder(output_folder);
		Utility.clear_folder(output_folder);
		target.run(output_folder);
	}

	public static List<string> get_namespace_path (Realm region) {
		var tokens = new List<string>();
		while(region != null && region.name != "root") {
			tokens.Insert(0, region.external_name ?? region.name);
            //region = region.parent;
		    break;
		}

		return tokens;
	}

}
}