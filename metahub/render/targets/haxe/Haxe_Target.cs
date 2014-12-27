using metahub.imperative;
using metahub.logic.schema;

namespace metahub.render.targets.haxe
{

public class Haxe_Target : Target{

	public Haxe_Target(Railway railway, Imp imp)
:base(railway, imp) {
	}

	override public void run (string output_folder) {
		foreach (var region in railway.regions.Values){
			foreach (var rail in region.rails.Values) {
				var trellis = rail.trellis;
				//trace(trellis.space.fullname);
				var space = Generator.get_namespace_path(rail.region);
				var dir = output_folder + "/" + space.join("/");
				Utility.create_folder(dir);

				var text = "package " + space.join(".")
					+ ";\n\nclass " + trellis.name + " {\n\n}\n\nclass "
					+ trellis.name + "_Actions {\n\n}\n";

				Utility.create_file(dir + "/" + trellis.name + ".hx", text);
			}
		}
	}
}
}