package metahub.parser;
//import metahub.code.functions.Functions;

struct Assignment_Source {
	string type,
	List<string> path,
	Object expression,
	string modifier
}

struct Reference_Or_Function {
	string type,
	//List<string> path,
	string name,
	Object expression,
	List<Object> inputs
}

class MetaHub_Context extends Context {

	//private static Map<string, Functions> function_Dictionary;

  public MetaHub_Context(definition) {
		super(definition);

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
			//foreach (var i in Reflect.fields(map)) {
				//function_map[i] = map[i];
			//}
		//}
	}

  public override Object perform_action (string name, Object data, Match match) {
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
        return reference(data, cast match);

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

  static Object start (Object data) {
    return {
			"type": "block",
			"expressions": data[1]
    };
  }

  static Object create_symbol (Object data) {
    return {
			type: "symbol",
			name: data[2],
			expression: data[6]
    };
  }

  static Object expression (List<Object> data, Match match) {
    if (data.Count() < 2)
      return data[0];

    Repetition_Match rep_match = cast match;
    string operator = cast rep_match.dividers[0].matches[1].get_data();

		if (operator == "|") {
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
				"name": operator,
				"inputs": data
			}
		}
  }

	static Object method (Object data) {
		return {
    type: "function",
    "name": data[1],
    "inputs": []
    }
	}

	static Object condition (Object data) {
		return {
			type: "condition",
			"first": data[0],
			"operator": Std.string(data[2][0]),
			//"operator": Std.string(function_map[data[2][0]]),
			"second": data[4]
    }
	}

	static Object optional_block (Object data) {
		return data[1];
	}

	static Object conditions (Object data, Match match) {
		Repetition_Match rep_match = cast match;
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

	static Object condition_block (Object data) {
		return data[2];
	}

	static Object if_statement (Object data) {
		return {
			type: "if",
			"condition": data[2],
			"action": data[4]
    }
	}

  static Object create_constraint (Object data) {
    return {
			type: "specific_constraint",
			path: data[0],
			expression: data[4]
    };
  }

  static Object create_node (Object data) {
    Object result = cast {
			type: "create_node",
			trellis: data[2]
    };

    if (data[3] != null && data[3].Count() > 0) {
			result.block = data[3][0];
      //result.set = data[4][0];
    }

    return result;
  }

  static Object reference (Object data, Repetition_Match match) {
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

  static Object long_block (Object data) {
		return {
			type: "block",
			expressions: data[2]
		};
  }

  static Object set_property (Object data) {
    Assignment_Source result = {
			type: "set_property",
			path: data[0],
			expression: data[6],
		};

		if (data[4].Count() > 0)
			result.modifier = Std.string(data[4][0]);

		return result;
  }

  static Object set_weight (Object data) {
		return {
			type: "weight",
			weight: data[0],
			statement: data[4]
		}
	}

  static Object value (Object data) {
    return {
    type: "literal",
    value: data
    };
  }

	static Object new_scope (Object data) {
    return {
			"type": "new_scope",
			"path": data[0],
			"expression": data[2]
		};
  }

	static Object constraint_block (Object data) {
    return data[2];
  }

	static Object constraint (Object data) {
    return {
			type: "constraint",
			reference: data[0],
			//operator: Std.string(function_map[data[2]]),
			operator: data[2],
			expression: data[4],
			lambda: data[5][0]
    };
  }

	static Object array_expression (Object data) {
    return {
			"type": "array",
			"expressions": data[2]
    };
  }

	static Object lambda_expression (Object data) {
    return {
			type: "lambda",
			parameters: data[1],
			expressions: data[3].expressions
    };
  }
	
	static Object parameters (Object data) {
    return data[2];
  }
	
	static Object function_scope (Object data) {
    return {
			type: "function_scope",
			expression: data[0],
			lambda: data[1]
    };
  }
	
}