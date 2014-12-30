using System.Collections.Generic;
using metahub.logic.schema;

namespace metahub.logic {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Scope {
	public Rail rail;
	public Scope parent;
	public Dictionary<string, Signature> variables = new Dictionary<string, Signature>();

	public Scope(Scope parent = null) {
		this.parent = parent;
		if (parent != null)
			rail = parent.rail;
	}

	public Signature find (string name) {
		if (variables.ContainsKey(name))
			return variables[name];

		if (parent != null)
			return parent.find(name);

		return null;
	}
}}