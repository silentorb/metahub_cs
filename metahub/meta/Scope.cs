using metahub.logic.schema.Rail;

namespace a {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Scope {
	public Rail rail;
	public Scope parent;
	public Dictionary<string, metahub.logic.schema.Signature> variables = new Dictionary<string, metahub.logic.schema.Signature>();

	public Scope(Scope parent = null) {
		this.parent = parent;
		if (parent != null)
			rail = parent.rail;
	}

	public void find (string name) {
		if (variables.ContainsKey(name))
			return variables[name];

		if (parent != null)
			return parent.find(name);

		return null;
	}
}}