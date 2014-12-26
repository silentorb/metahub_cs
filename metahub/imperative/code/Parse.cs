using System;
using System.Collections.Generic;
using metahub.logic.schema;
using metahub.meta.types;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Function_Call;
using Path = metahub.imperative.types.Path;

namespace metahub.imperative.code {

public class Parse
{

	public static Tie get_start_tie (Expression expression) {
		Path path = expression;
		metahub.imperative.types.Property_Expression property_expression = path.children[0];
		return property_expression.tie;
	}

	public static Tie get_end_tie (Expression expression) {
		var path = get_path(expression);
		var i = path.Count;
		while (--i >= 0) {
			if (!path[i].rail.trellis.is_value)
				return path[i];
		}

		throw new Exception("Could not find property inside expression path.");
	}

	//public static Tie get_end_tie (Expression expression) {
		//Path path = expression;
		//var i = path.children.Count();
		//while (--i >= 0) {
			//if (path.children[i].type == Expression_Type.property) {
				//metahub.imperative.types.Property_Expression property_expression = path.children[i];
				//if (property_expression.tie.rail.trellis.is_value)
					//continue;
//
				//return property_expression.tie;
			//}
		//}
//
		//throw new Exception("Could not find property inside expression path.");
	//}

	public static List<Tie> get_path (Expression expression) {
		switch (expression.type) {

			case Expression_Type.path:
				return get_path_from_array((expression, Path).children);

			case Expression_Type.array:
				return get_path_from_array((expression, Array_Expression).children);

			case Expression_Type.property:
				Property_Expression property_expression = expression;
				return [ property_expression.tie ];

			case Expression_Type.function_call:
				Function_Call function_call = expression;
				return [];
				//throw new Exception("Not supported.");
				//if (function_call.input == null)
					//return null;
					//throw new Exception("Not supported.");

				//return get_path(function_call.input);

			case Expression_Type.variable:
				return [];

			default:
				return [];
				//throw new Exception("Unsupported path expression type: " + expression.type);
		}
	}

	public static List<Tie> get_path_from_array (List<Expression> expressions) {
		List<Tie> result = new List<Tie>();
		foreach (var token in expressions) {
			result = result.concat(get_path(token));
		}

		return result;
	}

	public static List<Expression> normalize_path (Expression expression) {
		switch (expression.type) {

			case Expression_Type.path:
				Path path = expression;
				var result = [];
				foreach (var token in path.children) {
					result = result.concat(normalize_path(token));
				}
				return result;

			case Expression_Type.function_call:
				Function_Call function_call = expression;
				//if (function_call.input != null)
					//return normalize_path(function_call.input);

				return [ expression ];

			default:
				return [ expression ];
		}
	}


	public static Expression resolve (Expression expression) {
		switch (expression.type) {

			case Expression_Type.path:
				Path path = expression;
				return path.children[path.children.Count() - 1];

			case Expression_Type.function_call:
				throw new Exception("Not implemented.");

			default:
				return expression;
		}
	}

	//static void simplify_property_path (Path path) {
		//List<Tie> result = new List<Tie>();
		//foreach (var token in path.children) {
			//result = result.concat(get_path(token));
		//}
//
		//return result;
	//}

	public static List<Tie> reverse_path (List<Tie> path) {
		var result = path.map((t)=>{
			return t.other_tie;
		});
		result.reverse();
		return result;
	}

}}