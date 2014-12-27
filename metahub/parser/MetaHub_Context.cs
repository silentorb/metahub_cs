using System;
using System.Collections.Generic;
using System.Linq;
using metahub.metahub.parser.types;

namespace metahub.parser
{
//import metahub.code.functions.Functions;

class Assignment_Source
{
    public string type;
    public List<string> path;
    public object expression;
    public string modifier;
}

class Reference_Or_Function
{
    public string type;
	//List<string> path,
    public string name;
    public object expression;
    public List<object> inputs;
}
public class MetaHub_Context : Context {

	//private static Map<string, Functions> function_Dictionary;

  public MetaHub_Context(Definition definition)
:base(definition) {

	}

  public override object perform_action (string name, object data, Match match) {
    //var name = match.pattern.name;
    switch (match.pattern.name)
    {
      case "start":
        return start(data);

      case "create_symbol":
        return create_symbol(data);

      case "create_node":
        return create_node(data);

      case "create_constraint":
        return create_constraint(data);

      case "Node":
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
        return condition((object[])data);

      case "conditions":
        return conditions((object[])data, match);

      case "condition_block":
        return condition_block(data);

      case "if":
        return if_statement(data);

      case "string":
        return ((object[])data)[1];

	case "bool":
		return (string)data == "true";

      case "int":
        return int.Parse((string)data);

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

  static Parser_Block start (object[] data) {
    return new Parser_Block {
			type= "block",
			expressions=(Parser_Item[]) data[1]
    };
  }

  static Parser_Symbol create_symbol (object[] data) {
      return new Parser_Symbol
      {
			type= "symbol",
			name= (string)data[2],
			expression=(Parser_Item) data[6]
    };
  }

  static object expression (object[] data, Match match) {
    if (data.Length < 2)
      return data[0];

    var rep_match = (Repetition_Match)match;
    string op = (string)rep_match.dividers[0].matches[1].get_data();

		if (op == "|") {
			var function_name = data.Last();
		    data = data.Take(data.Length - 1).ToArray();

		    return new Parser_Function_Call
		        {
		            type = "function",
		            name = function_name.children[0].name,
		            inputs = data
		        };
		}
		else
		{
		    return new Parser_Function_Call
		        {
		            type = "function",
		            name = op,
		            inputs = data
		        };
		}
  }

    //static object method (object data) {
    //    return {
    //type= "function",
    //"name"= data[1],
    //"inputs"= []
    //}
    //}

	static Parser_Condition condition (object[] data)
	{
	    return new Parser_Condition
	        {
	            type = "condition",
	            first = (Parser_Item) data[0],
	            op = ((object[]) data[2])[0].ToString(),
	            //"op"= Std.string(function_map[data[2][0]]),
                second = (Parser_Item)data[4]
	        };
	}

	static object optional_block (object[] data) {
		return data[1];
	}

	static object conditions (object[] data, Match match) {
		var rep_match = (Repetition_Match)match;
		if (data.Length > 1) {
			string symbol = (string)rep_match.dividers[0].matches[1].get_data();
			string divider = null;
			switch(symbol) {

				case "&&": divider = "and";
			        break;
				
                case "||": divider = "or";
			        break;

				default: throw new Exception("Invalid condition group joiner: " + symbol + ".");
			}
		    return new Parser_Conditions
		        {
		            type = "conditions",
		            conditions = (Parser_Condition[]) data,
		            mode = divider
		        };
		}
		else {
            //throw new Exception("Not implemented.");
            return data[0];
		}
	}

	static object condition_block (object[] data) {
		return data[2];
	}

	static Parser_If if_statement (object[] data)
	{
	    return new Parser_If
	        {
	            type = "if",
	            condition = (Parser_Condition) data[2],
	            action = (Parser_Item) data[4]
	        };
	}

  static Parser_Constraint create_constraint (object[] data) {
      return new Parser_Constraint
      {
			type= "specific_constraint",
			path=(Parser_Item) data[0],
            expression = (Parser_Item)data[4]
    };
  }

  //static object create_node (object data) {
  //  object result = {
  //          type= "create_node",
  //          trellis= data[2]
  //  };

  //  if (data[3] != null && data[3].Count > 0) {
  //          result.block = data[3][0];
  //    //result.set = data[4][0];
  //  }

  //  return result;
  //}

  static object reference (object[] data, Repetition_Match match) {
		var dividers = match.dividers.Select((d)=>d.matches[0].get_data() );
//
		//if (data.Count == 1) {
			//return {
				//type= "reference",
				//path= [ data[0] ]
			//}
		//}

      var tokens = new List<Reference_Or_Function>();
      
            //data[0].type == "array"
            //? data[0]
            //: new Parser_Reference {
            //    type= "reference",
            //    name= (string) data[0]
            //}			

		for (var i = 1; i <data.Length; ++i) {
            var token = data[i];
			var divider = dividers[i - 1];
			if (divider == ".") {
				tokens.Add({
					type= "reference",
					name= token
				});
			}
			else if (divider == "|") {
				tokens.Add({
					type= "function",
					name= token
				});
			}
			else {
				throw new Exception("Invalid divider= " + divider);
			}
		}

		var result = {
			type= "path",
			children= tokens
		};

		return result;
  }

  static object long_block (object data) {
		return {
			type= "block",
			expressions= data[2]
		};
  }

  static object set_property (object data) {
    Assignment_Source result = {
			type= "set_property",
			path= data[0],
			expression= data[6],
		};

		if (data[4].Count > 0)
			result.modifier = Std.string(data[4][0]);

		return result;
  }

  static object set_weight (object data) {
		return {
			type= "weight",
			weight= data[0],
			statement= data[4]
		}
	}

  static object value (object data) {
    return {
    type= "literal",
    value= data
    };
  }

	static object new_scope (object data) {
    return {
			"type"= "new_scope",
			"path"= data[0],
			"Node"= data[2]
		};
  }

	static object constraint_block (object data) {
    return data[2];
  }

	static object constraint (object data) {
    return {
			type= "constraint",
			reference= data[0],
			//op= Std.string(function_map[data[2]]),
			op= data[2],
			expression= data[4],
			lambda= data[5][0]
    };
  }

	static object array_expression (object data) {
    return {
			"type"= "array",
			"expressions"= data[2]
    };
  }

	static object lambda_expression (object data) {
    return {
			type= "lambda",
			parameters= data[1],
			expressions= data[3].expressions
    };
  }
	
	static object parameters (object data) {
    return data[2];
  }
	
	static object function_scope (object data) {
    return {
			type= "function_scope",
			expression= data[0],
			lambda= data[1]
    };
  }
	
}
}