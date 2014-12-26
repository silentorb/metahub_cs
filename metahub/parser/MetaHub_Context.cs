namespace metahub.parser
{
//import metahub.code.functions.Functions;

struct Assignment_Source {
	string type,
	List<string> path,
	object expression,
	string modifier
}

struct Reference_Or_Function {
	string type,
	//List<string> path,
	string name,
	object expression,
	List<object> inputs
}
public class MetaHub_Context : Context {

	//private static Map<string, Functions> function_Dictionary;

  public MetaHub_Context(definition)
:base(definition) {

		//if (function_map == null) {
			//function_map = new Dictionary<string, Functions>();
			//var map = {
				//"+": Functions.add,
				//"-": Functions.subtract,
				//"*": Functions.multiply,
				//"/": Functions.divide,
//
				//"+=": Functions.add_equals,
				//"-=": Functions.subtract_equals,
				//"*=": Functions.multiply_equals,
				//"/=": Functions.divide_equals,
//
				//"=": Functions.equals,
				//"<": Functions.lesser_than,
				//">": Functions.greater_than,
				//"<=": Functions.lesser_than_or_equal_to,
				//">=": Functions.greater_than_or_equal_to,
			//}
//
			//foreach (var i in map.Keys) {
				//function_map[i] = map[i];
			//}
		//}
	}

  public override object perform_action (string name, object data, Match match) {
    var name = match.pattern.name;
    switch(name) {
      case "start":
        return start(data);

      case "create_symbol":
        return create_symbol(data);

      case "create_node":
        return create_node(data);

      case "create_constraint":
        return create_constraint(data);

      case "expression":
        return expression(data, match);

      case "method":
        return method(data);

      case "reference":
        return reference(data, match);

      case "long_block":
        return long_block(data);

      case "set_property":
        return set_property(data);

      case "new_scope":
        return new_scope(data);

      case "constraint_block":
        return constraint_block(data);

      case "constraint":
        return constraint(data);

      case "condition":
        return condition(data);

      case "conditions":
        return conditions(data, match);

      case "condition_block":
        return condition_block(data);

      case "if":
        return if_statement(data);

      case "string":
        return data[1];

			case "bool":
				return data == "true" ? true : false;

      case "int":
        return Std.parseInt(data);

      case "value":
        return value(data);

      case "optional_block":
        return optional_block(data);

			case "set_weight":
        return set_weight(data);

			case "array":
        return array_expression(data);

			case "lambda":
        return lambda_expression(data);
				
			case "parameters":
				return parameters(data);
				
			case "function_scope":
				return function_scope(data);

//      default:
//        throw new Exception("Invalid parser method: " + name + ".");
    }

    return data;
  }

  static object start (object data) {
    return {
			"type": "block",
			"expressions": data[1]
    };
  }

  static object create_symbol (object data) {
    return {
			type: "symbol",
			name: data[2],
			expression: data[6]
    };
  }

  static object expression (List<object> data, Match match) {
    if (data.Count() < 2)
      return data[0];

    Repetition_Match rep_match = match;
    string op = rep_match.dividers[0].matches[1].get_data();

		if (op == "|") {
			var function_name = data.pop();
			return {
				type: "function",
				name: function_name.children[0].name,
				inputs: data
			}
		}
		else {
			return {
				type: "function",
				"name": op,
				"inputs": data
			}
		}
  }

	static object method (object data) {
		return {
    type: "function",
    "name": data[1],
    "inputs": []
    }
	}

	static object condition (object data) {
		return {
			type: "condition",
			"first": data[0],
			"op": Std.string(data[2][0]),
			//"op": Std.string(function_map[data[2][0]]),
			"second": data[4]
    }
	}

	static object optional_block (object data) {
		return data[1];
	}

	static object conditions (object data, Match match) {
		Repetition_Match rep_match = match;
		if (data.Count() > 1) {
			string symbol = rep_match.dividers[0].matches[1].get_data();
			string divider = null;
			switch(symbol) {
				case "&&": divider = "and";
				case "||": divider = "or";
				default: throw new Exception("Invalid condition group joiner: " + symbol + ".");
			}
			return {
				type: "conditions",
				"conditions": data,
				"mode": divider
			}
		}
		else {
			return data[0];
		}
	}

	static object condition_block (object data) {
		return data[2];
	}

	static object if_statement (object data) {
		return {
			type: "if",
			"condition": data[2],
			"action": data[4]
    }
	}

  static object create_constraint (object data) {
    return {
			type: "specific_constraint",
			path: data[0],
			expression: data[4]
    };
  }

  static object create_node (object data) {
    object result = {
			type: "create_node",
			trellis: data[2]
    };

    if (data[3] != null && data[3].Count() > 0) {
			result.block = data[3][0];
      //result.set = data[4][0];
    }

    return result;
  }

  static object reference (object data, Repetition_Match match) {
		var dividers = Lambda.array(Lambda.map(match.dividers, (d)=>{ return d.matches[0].get_data(); } ));
//
		//if (data.Count() == 1) {
			//return {
				//type: "reference",
				//path: [ data[0] ]
			//}
		//}

		List<Reference_Or_Function> tokens = [
			data[0].type == "array"
			? data[0]
			: {
				type: "reference",
				name: data[0]
			}			
		];

		foreach (var i in 1...data.Count()) {
			var token = data[i];
			var divider = dividers[i - 1];
			if (divider == ".") {
				tokens.Add({
					type: "reference",
					name: token
				});
			}
			else if (divider == "|") {
				tokens.Add({
					type: "function",
					name: token
				});
			}
			else {
				throw new Exception("Invalid divider: " + divider);
			}
		}

		var result = {
			type: "path",
			children: tokens
		};

		return result;
  }

  static object long_block (object data) {
		return {
			type: "block",
			expressions: data[2]
		};
  }

  static object set_property (object data) {
    Assignment_Source result = {
			type: "set_property",
			path: data[0],
			expression: data[6],
		};

		if (data[4].Count() > 0)
			result.modifier = Std.string(data[4][0]);

		return result;
  }

  static object set_weight (object data) {
		return {
			type: "weight",
			weight: data[0],
			statement: data[4]
		}
	}

  static object value (object data) {
    return {
    type: "literal",
    value: data
    };
  }

	static object new_scope (object data) {
    return {
			"type": "new_scope",
			"path": data[0],
			"expression": data[2]
		};
  }

	static object constraint_block (object data) {
    return data[2];
  }

	static object constraint (object data) {
    return {
			type: "constraint",
			reference: data[0],
			//op: Std.string(function_map[data[2]]),
			op: data[2],
			expression: data[4],
			lambda: data[5][0]
    };
  }

	static object array_expression (object data) {
    return {
			"type": "array",
			"expressions": data[2]
    };
  }

	static object lambda_expression (object data) {
    return {
			type: "lambda",
			parameters: data[1],
			expressions: data[3].expressions
    };
  }
	
	static object parameters (object data) {
    return data[2];
  }
	
	static object function_scope (object data) {
    return {
			type: "function_scope",
			expression: data[0],
			lambda: data[1]
    };
  }
	
}
}