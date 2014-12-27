using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.logic.schema;
using metahub.meta.types;
using metahub.parser.types;
using metahub.schema;
using Constraint = metahub.meta.types.Constraint;


namespace metahub.meta {
class Conditions_Source
{
    public string type;
    public List<object> conditions;
    public string mode;
}

public class Coder {
  Railway railway;

  public Coder(Railway railway) {
    this.railway = railway;
  }

  public Node convert_expression (object source, Node previous, Scope scope) {

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

	public Node convert_statement (object source, Scope scope, Signature type = null) {

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

  Node constraint (Parser_Constraint source, Scope scope) {
		//var reference = Reference.from_scope(source.path, scope);
		var reference = convert_expression(source.reference, null, scope);
		Node back_reference = null;
		var operator_name = source.op;
		if (new List<string>{"+=", "-=", "*=", "/="}.Contains(operator_name)) {
			//operator_name = operator_name.substring(0, operator_name.Count - 7);
			back_reference = reference;
		}
		var expression = Parse.resolve(convert_expression(source.expression, null, scope));

		return new Constraint(reference, expression, operator_name,
			source.lambda != null ? create_lambda(source.lambda, scope, new List<Node> {reference, expression }) : null
		);
		
		
  }

  Node create_block (Parser_Block source, Scope scope) {
		var count = source.expressions.Length;
    if (count == 0)
			return new Block();

		var fields = source.expressions;

		if (count == 1) {
      return convert_statement(fields.First(), scope);
		}
    Block block = new Block();

    foreach (var e in fields) {
      var child = source.expressions[e];
      block.children.Add(convert_statement(child, scope));
    }

    return block;
  }

  Node create_literal (Literal_Value source, Scope scope) {
    var type = get_type(source.value);
    //return new metahub.code.expressions.Literal(source.value, type);
		return new Literal_Value(source.value);
  }

  Node function_expression (Parser_Function_Call source, Scope scope, string name, Node previous) {
    var expressions = source.inputs;
		if (source.inputs.Length > 0)
			throw new Exception("Not supported.");

    //var inputs = Lambda.array(Lambda.map(expressions, (e)=> convert_expression(e, scope)));

		return new Function_Call(name, previous, railway);
		//var info = Function_Call.get_function_info(name, hub);
    //return new metahub.code.expressions.Function_Call(name, info, inputs, hub);
  }

    //List<string> extract_path (object path) {
    //    List<string> result = new List<string>();
    //    for (var i = 1; i < path) {
    //        result.Add(path[i]);
    //    }

    //    return result;
    //}

  Node create_path(Parser_Block source, Node previous, Scope scope)
  {
		Rail rail = scope.rail;
		Node expression = null;
		List<Node> children = new List<Node>();
		var expressions = source.expressions;
		if (expressions.Length == 0)
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
                    break;
                
                case "reference":
					var variable = scope.find(item.name);
					if (variable != null) {
						previous = new Variable(item.name);
                        //if (variable.rail == null)
                        //    throw new Exception("");
                        //rail = variable.rail;
                        throw new Exception("Not implemented");
					}
					else {
						Tie tie = rail.get_tie_or_error(item.name);
						previous = new Property_Reference(tie);
						if (tie.other_rail != null)
							rail = tie.other_rail;
					}
			        break;

				case "array":
					var items = ((Parser_Block)item).expressions;
					Node token = null;
					var sub_array = items.Select(i => convert_expression(i, token, scope)).ToList();
			        previous = new Array_Expression(sub_array);
                    break;
                
				default:
					throw new Exception("Invalid path token type: " + item.type);
			}

			children.Add(previous);
		}
		return new Reference_Path(children);
  }

  static Signature get_type (object value) {
    if (value is int)
    {
        return new Signature
            {
                type = Kind.unknown,
                is_numeric = 1
            };
    }

    if (value is float)
      return new Signature(Kind.Float );

    if (value is bool)
      return new Signature(Kind.Bool);

    if (value is string)
        return new Signature(Kind.String);

    throw new Exception("Could not find type.");
  }

	Node new_scope (Parser_Scope source, Scope scope) {
		var path = source.path;
		if (path.Length == 0)
			throw new Exception("Scope path is empty for node creation.");

		Node expression = null;
		Scope new_scope = new Scope();
		if (path.Length == 1 && path[0] == "new") {
			//new_scope_definition.only_new = true;
			expression = convert_statement(source.expression, new_scope);
			return new Scope_Expression(new_scope,new List<Node> {expression});
			//return new Scope_Expression(Node, new_scope_definition);
		}

		var rail = railway.resolve_rail_path(path);// hub.schema.root_namespace.get_namespace(path);
		//var trellis = hub.schema.get_trellis(path[path.Count - 1], namespace);

		//if (rail != null) {
			new_scope.rail = rail;
			expression = convert_statement(source.expression, new_scope);
			//return new Scope_Expression(Node, new_scope_definition);
            return new Scope_Expression(new_scope, new List<Node> { expression });
		//}
		//else {
			//throw new Exception("Not implemented.");
			////var symbol = scope.find(source.path);
			////new_scope_definition.symbol = symbol;
			////new_scope_definition.trellis = symbol.get_trellis();
			////Node = convert_statement(source.Node, new_scope_definition);
			////return new Node_Scope(Node, new_scope_definition);
		//}
	}

	//Node weight (object source, Scope scope) {
		//return new Set_Weight(source.weight, convert_statement(source.statement, scope));
  //}

  Node create_array (Parser_Block source, Scope scope) {
        return new Block(source.expressions.Select((e) => convert_expression(e, null, scope)));
  }

  Lambda create_lambda (Parser_Lambda source, Scope scope, List<Node> constraint_expressions) {
		var expressions = source.expressions;
		Scope new_scope = new Scope(scope);
		var parameters = source.parameters;
		int i = 0;
		foreach (var parameter in parameters) {
			var expression = constraint_expressions[i];
			var path = Parse.normalize_path(expression);
			new_scope.variables[parameter] = path[path.Count - 1].get_signature();
			++i;
		}

		return new Lambda(new_scope, parameters.Select((p)=> new Parameter(p, null))
			, expressions.Select((e)=> convert_statement(e, new_scope))
		);
  }

  Node function_scope (Parser_Function_Scope source, Scope scope) {
		var expression = convert_expression(source.expression, null, scope);
		var path = (Reference_Path)expression;
		var token = path.children[path.children.Count - 2];
		return new Function_Scope(expression,
			create_lambda(source.lambda, scope, new List<Node>{ token, token })
		);
  }

}}