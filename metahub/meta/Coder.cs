using metahub.Hub;
using metahub.imperative.code.Parse;
using metahub.logic.schema.Signature;
using metahub.logic.schema.Railway;
using metahub.logic.schema.Tie;
using metahub.meta.types.Array_Expression;
using metahub.meta.types.Block;
using metahub.meta.types.Constraint;
using metahub.meta.types.Expression_Type;
using metahub.meta.types.Expression;
using metahub.meta.types.Function_Call;
using metahub.meta.types.Function_Scope;
using metahub.meta.types.Literal;
using metahub.meta.types.Parameter;
using metahub.meta.types.Path;
using metahub.meta.types.Property_Expression;
using metahub.meta.types.Scope_Expression;
using metahub.meta.types.Variable;
using metahub.schema.Kind;
using metahub.schema.Namespace;
using metahub.logic.schema.Rail;

namespace a {
struct Conditions_Source {
	string type,
	List<Object> conditions,
	string mode
}

class Coder {
  Railway railway;

  public Coder(Railway railway) {
    this.railway = railway;
  }

  public Expression convert_expression (Object source, Expression previous, Scope scope) {

    switch(source.type) {
			case "block":
        return create_block(source, scope);
      case "literal":
        return create_literal(source, scope);
      case "path":
        return create_path(source, previous, scope);
			//case "reference":
        //return create_general_reference(source, scope);
      case "function":
				throw new Exception("Not supported.");
        return function_expression(source, scope, source.name, previous);

			//case "create_node":
        //return create_node(source, scope);
			//case "conditions":
        //return conditions(source, scope);
			//case "condition":
        //return condition(source, scope);
			case "array":
        return create_array(source, scope);
			//case "lambda":
        //return create_lambda(source, scope);
    }

    throw new Exception("Invalid block: " + source.type);
  }

	public Expression convert_statement (Object source, Scope scope, Signature type = null) {

    switch(source.type) {
      case "block":
        return create_block(source, scope);
      //case "symbol":
        //return create_symbol(source, scope);
			case "new_scope":
				return new_scope(source, scope);
			//case "create_node":
        //return create_node(source, scope);
			//case "if":
        //return if_statement(source, scope);
      case "constraint":
        return constraint(source, scope);
      case "function_scope":
        return function_scope(source, scope);
			//case "weight":
        //return weight(source, scope);
		}

    throw new Exception("Invalid block: " + source.type);
  }

  Expression constraint (Object source, Scope scope) {
		//var reference = Reference.from_scope(source.path, scope);
		var reference = convert_expression(source.reference, null, scope);
		Expression back_reference = null;
		var operator_name = source.operator;
		if (["+=", "-=", "*=", "/="].Contains(operator_name)) {
			//operator_name = operator_name.substring(0, operator_name.Count() - 7);
			back_reference = reference;
		}
		var expression = Parse.resolve(convert_expression(source.expression, null, scope));

		return new Constraint(reference, expression, operator_name,
			source.lambda != null ? cast create_lambda(source.lambda, scope, [ reference, expression ]) : null
		);
		
		
  }

  Expression create_block (Object source, Scope scope) {
		var count = source.expressions.Keys.Count();
    if (count == 0)
			return new Block();

		var fields = source.expressions.Keys;

		if (count == 1) {
			var expression = source.expressions[fields[0]];
      return convert_statement(expression, scope);
		}
    Block block = new Block();

    foreach (var e in fields) {
      var child = source.expressions[e];
      block.children.Add(convert_statement(child, scope));
    }

    return block;
  }

  Expression create_literal (Object source, Scope scope) {
    var type = get_type(source.value);
    //return new metahub.code.expressions.Literal(source.value, type);
		return new Literal(source.value);
  }

  Expression function_expression (Object source, Scope scope, string name, Expression previous) {
    List<Object> expressions = source.inputs;
		if (source.inputs.Count() > 0)
			throw new Exception("Not supported.");

    //var inputs = Lambda.array(Lambda.map(expressions, (e)=> convert_expression(e, scope)));

		return new Function_Call(name, previous, railway);
		//var info = Function_Call.get_function_info(name, hub);
    //return new metahub.code.expressions.Function_Call(name, info, inputs, hub);
  }

	List<string> extract_path (Object path) {
		List<string> result = new List<string>();
		foreach (var i in 1...path.Count()) {
			result.Add(path[i]);
		}

		return result;
	}

  Expression create_path (Object source, Expression previous, Scope scope) {
		Rail rail = scope.rail;
		Expression expression = null;
		List<Expression> children = new List<Expression>();
		List<Object> expressions = source.children;
		if (expressions.Count() == 0)
			throw new Exception("Empty reference path.");

		if (expressions[0].type == "reference" && rail.get_tie_or_null(expressions[0].name) == null
			&& scope.find(expressions[0].name) == null) {
				throw new Exception("Not supported.");
		}

		foreach (var item in expressions) {
			switch (item.type) {
				case "function":
					previous = new Function_Call(item.name, previous, railway);
					//var info = Function_Call.get_function_info(item.name, hub);
					//children.Add(new metahub.code.expressions.Function_Call(item.name, info, [], hub));
				case "reference":
					var variable = scope.find(item.name);
					if (variable != null) {
						previous = new Variable(item.name);
						if (variable.rail == null)
							throw "";
						rail = variable.rail;
					}
					else {
						Tie tie = cast rail.get_tie_or_error(item.name);
						previous = new Property_Expression(tie);
						if (tie.other_rail != null)
							rail = tie.other_rail;
					}
				case "array":
					List<Object> items = cast item.expressions;
					Expression token = null;
					var sub_array = [];
					foreach (var item in items) {
						var token = convert_expression(item, token, scope);
						sub_array.Add(token);
					}
					previous = new Array_Expression(sub_array);

				default:
					throw new Exception("Invalid path token type: " + item.type);
			}

			children.Add(previous);
		}
		return new Path(children);
  }

  static Signature get_type (Object value) {
    if (Std.is(value, int)) {
      return {
					type: Kind.unknown,
					is_numeric: 1
			}
		}

    if (Std.is(value, float))
      return { type: Kind.Float };

    if (Std.is(value, bool))
      return { type: Kind.Bool };

    if (Std.is(value, string))
      return { type: Kind.String };

    throw new Exception("Could not find type.");
  }

	Expression new_scope (Object source, Scope scope) {
		List<string> path = source.path;
		if (path.Count() == 0)
			throw new Exception("Scope path is empty for node creation.");

		Expression expression = null;
		Scope new_scope = new Scope();
		if (path.Count() == 1 && path[0] == "new") {
			//new_scope_definition.only_new = true;
			expression = convert_statement(source.expression, new_scope);
			return new Scope_Expression(new_scope, [expression]);
			//return new Scope_Expression(expression, new_scope_definition);
		}

		var rail = railway.resolve_rail_path(path);// hub.schema.root_namespace.get_namespace(path);
		//var trellis = hub.schema.get_trellis(path[path.Count() - 1], namespace);

		//if (rail != null) {
			new_scope.rail = rail;
			expression = convert_statement(source.expression, new_scope);
			//return new Scope_Expression(expression, new_scope_definition);
			return new Scope_Expression(new_scope, [expression]);
		//}
		//else {
			//throw new Exception("Not implemented.");
			////var symbol = scope.find(source.path);
			////new_scope_definition.symbol = symbol;
			////new_scope_definition.trellis = symbol.get_trellis();
			////expression = convert_statement(source.expression, new_scope_definition);
			////return new Node_Scope(expression, new_scope_definition);
		//}
	}

	//Expression weight (Object source, Scope scope) {
		//return new Set_Weight(source.weight, convert_statement(source.statement, scope));
  //}

  Expression create_array (Object source, Scope scope) {
		List<Object> expressions = source.expressions;
		return new Block(expressions.map((e)=> convert_expression(e, null, scope)));
  }

  Expression create_lambda (Object source, Scope scope, List<Expression> constraint_expressions) {
		List<Object> expressions = source.expressions;
		Scope new_scope = new Scope(scope);
		List<string> parameters = source.parameters;
		int i = 0;
		foreach (var parameter in parameters) {
			var expression = constraint_expressions[i];
			var path = Parse.normalize_path(expression);
			new_scope.variables[parameter] = path[path.Count() - 1].get_signature();
			++i;
		}

		return new metahub.meta.types.Lambda(new_scope, parameters.map((p)=> new Parameter(p, null))
			, expressions.map((e)=> convert_statement(e, new_scope))
		);
  }

  Expression function_scope (Object source, Scope scope) {
		var expression = convert_expression(source.expression, null, scope);
		Path path = cast expression;
		var token = path.children[path.children.Count() - 2];
		return new Function_Scope(expression,
			cast create_lambda(source.lambda, scope, [ token, token ])
		);
  }

}}