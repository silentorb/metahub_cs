using System.Collections.Generic;
using metahub.schema;

namespace metahub.logic.schema {
/**
 * ...
 * @author Christopher W. Johnson
 */

class Tie {

	public Rail rail;
	public Property property;
	public string name;
	public string tie_name;
	public Rail other_rail;
	public Tie other_tie;
	public bool is_value = false;
	public bool has_getter = false;
	public bool has_set_post_hook = false;
	public Kind type;
	public List<IRange> ranges = new List<IRange>();

	public List<Constraint> constraints = new List<Constraint>();

	public Tie(Rail rail, Property property) {
		this.rail = rail;
		this.type = property.type;
		this.property = property;
		tie_name = name = property.name;
	}

	public void initialize_links () {
		if (property.other_trellis != null) {
			other_rail = rail.railway.get_rail(property.other_trellis);
			is_value = property.other_trellis.is_value;
			if (other_rail != null && property.other_property != null && other_rail.all_ties.ContainsKey(property.other_property.name)) {
				other_tie = other_rail.all_ties[property.other_property.name];
				//other_tie.other_rail = rail;
				//other_tie.other_tie = this;
			}
		}

		var additional = rail.property_additional[name];
		if (additional != null && additional.hooks != null) {
			has_set_post_hook = additional.hooks.set_post != null;
		}
	}

	public void has_setter () {
		return (property.type != Kind.list && constraints.Count() > 0)
		|| has_set_post_hook || (property.type == Kind.reference && !is_inherited());
	}

	public string get_setter_post_name () {
		return "set_" + name + "_post";
	}

	public Signature get_signature () {
		return {
			type: property.type,
			rail: other_rail,
			is_value: is_value
		};
	}

	public Signature get_other_signature () {
		if (other_rail == null)
			throw new Exception("get_other_signature() can only be called on lists or references.");

		var other_type = other_tie != null
		? other_tie.type
		: type == Kind.list ? Kind.reference : Kind.list;

		return {
			type: other_type,
			rail: other_rail,
			is_value: is_value
		};
	}

	public bool is_inherited () {
		return rail.parent != null && rail.parent.all_ties.ContainsKey(name);
	}

	public void finalize () {
		determine_range();
	}

	void determine_range () {
		//if (type != Kind.Float)
			//return;
		if (type == Kind.list)
			return;

		Dictionary<string, { min:{Expression expression}, max:{Expression expression}, List<Tie> path }> pairs = new Dictionary<string, { min:{Expression expression}, max:{Expression expression}, List<Tie> path }>();
		//Constraint min = null;
		//Constraint max = null;

		foreach (var constraint in constraints) {
			var path = Parse.get_path(constraint.reference);
			path.shift();
			var path_name = path.map((t)=> t.name).join(".");
			if (!pairs.ContainsKey(path_name)) {
				pairs[path_name] = {
					min: null,
					max: null,
					path: path
				};
			}

			if (constraint.operator == "in") {
				metahub.meta.types.Block args = cast constraint.expression;
				pairs[path_name].min = { expression: args.children[0] };
				pairs[path_name].max = { expression: args.children[1] };
			}
			else if (constraint.operator == ">" || constraint.operator == ">=") {
				pairs[path_name].min = constraint;
			}
			else if (constraint.operator == "<" || constraint.operator == "<=") {
				pairs[path_name].max = constraint;
			}
		}

		foreach (var pair in pairs) {
			if (pair.min != null && pair.max != null) {
				//trace("range", fullname());
				ranges.Add(new Range_Float(
					get_expression_float(pair.min.expression),
					get_expression_float(pair.max.expression), pair.path));
			}
		}
	}

	static float get_expression_float (Expression expression){
		metahub.imperative.types.Literal conversion = cast expression;
		return conversion.value;
	}

	public Rail get_abstract_rail () {
		return rail;
	}

	public string fullname () {
		return rail.name + "." + name;
	}

	public Object get_default_value () {
		if (other_rail != null && other_rail.default_value != null)
			return other_rail.default_value;

		return property.get_default();
	}
}}