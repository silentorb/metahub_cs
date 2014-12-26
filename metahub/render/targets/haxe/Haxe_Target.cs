package metahub.render.targets.haxe ;
using metahub.imperative.Imp;
using metahub.logic.schema.Railway;
using metahub.Hub;
using metahub.schema.Namespace;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Haxe_Target extends Target{

	public Haxe_Target(Railway railway, Imp imp) {
		super(railway, imp);
	}

	override public void run (string output_folder) {
		foreach (var region in railway.regions){
			foreach (var rail in region.rails) {
				var trellis = rail.trellis;
				//trace(trellis.space.fullname);
				var namespace = Generator.get_namespace_path(rail.region);
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