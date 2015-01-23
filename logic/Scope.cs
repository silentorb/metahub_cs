using System.Collections.Generic;
using metahub.logic.schema;
using metahub.logic.nodes;

namespace metahub.logic {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Scope {
	public Rail rail;
	public Scope parent;
	public Dictionary<string, Signature> variables = new Dictionary<string, Signature>();
    public bool is_map = false;
    public Node[] caller;
    public Constraint_Group group;
    public Signature[] parameters;
    public Constraint_Scope constraint_scope;

	public Scope(Scope parent = null) {
		this.parent = parent;
	    if (parent != null)
	    {
	        rail = parent.rail;
	        constraint_scope = parent.constraint_scope;
	    }
	}

	public Signature find (string name) {
		if (variables.ContainsKey(name))
			return variables[name];

		if (parent != null)
			return parent.find(name);

		return null;
	}
}}