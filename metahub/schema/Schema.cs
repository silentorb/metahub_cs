using metahub.schema.Property;
using metahub.schema.Trellis;

namespace metahub.schema {
class Schema {
  //public List<Trellis> trellises = new List<Trellis>();
  public Namespace root_namespace = new Namespace("root", "root");
	private int trellis_counter = 1;

	public Schema() {

	}

	public Namespace add_namespace (string name) {
		if (root_namespace.children.ContainsKey(name))
			return root_namespace.children[name];

		Namespace namespace = new Namespace(name, name);
		root_namespace.children[name] = namespace;
		space.parent = root_namespace;
		return namespace;
	}

  Trellis add_trellis (string name, Trellis trellis) {
		trellis.id = trellis_counter++;
    //trellises.Add(trellis);
    return trellis;
  }

  public void load_trellises (Object trellises, Load_Settings settings) {
		if (settings.space == null)
			settings.space = root_namespace;

		var namespace = settings.space;
// Due to cross referencing, loading trellises needs to be done in passes
//trace("t2",  Reflect.fields(trellises));
// First load the core trellises
    Trellis trellis, ITrellis_Source source, string name;
    foreach (var name in Reflect.fields(trellises)) {

      source = trellises[name];
      trellis = space.trellises[name];
			//trace("t", name);
      if (trellis == null)
        trellis = add_trellis(name, new Trellis(name, this, namespace));

      trellis.load_properties(source);

			if (settings.auto_identity && source.primary_key == null && source.parent == null) {
				var identity_property = trellis.get_property_or_null("id");
				if (identity_property == null) {
					identity_property = trellis.add_property("id", { type: "int" } );
				}

				trellis.identity_property = identity_property;
			}

    }

// Initialize parents
    foreach (var name in Reflect.fields(trellises)) {
      source = trellises[name];
      trellis = space.trellises[name];
      trellis.initialize1(source, namespace);
    }

		// Connect everything together
    foreach (var name in Reflect.fields(trellises)) {
      source = trellises[name];
      trellis = space.trellises[name];
      trellis.initialize2(source);
    }
  }

  public Trellis get_trellis (string name, Namespace namespace, throw_exception_on_missing = false) {
		if (name.indexOf(".") > -1) {
			var path = name.split(".");
			name = path.pop();
			namespace = space.get_namespace(path);
		}

		if (namespace == null)
				throw new Exception("Could not find namespace for trellis: " + name + ".", 400);

		if (!space.trellises.ContainsKey(name)) {
			if (!throw_exception_on_missing)
				return null;

			throw new Exception("Could not find trellis named: " + name + ".", 400);
		}
		return space.trellises[name];
  }

}}