using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Scope = metahub.meta.Scope;
using Node_Type = metahub.meta.types.Node_Type;
using Node = metahub.meta.types.Node;

namespace metahub.imperative {

public class Imp
{
	public Railway railway;
	public List<Constraint> constraints = new List<Constraint>();
	public List<Dungeon> dungeons = new List<Dungeon>();
	Dictionary<Rail, Dungeon> rail_map = new Dictionary<Rail, Dungeon>();

	public Imp(Hub hub, string target_name)
	{
		railway = new Railway(hub, target_name);
	}

	public void run (metahub.meta.types.Node root, Target target) {
		process(root, null);
		generate_code(target);

		foreach (var constraint in constraints) {
			implement_constraint(constraint);
		}

		flatten();
		
		post_analyze();
	}

	public void generate_code (Target target) {
		
		foreach (var region in railway.regions.Values) {
			if (region.is_external)
				continue;

			foreach (var rail in region.rails.Values) {
				if (rail.is_external)
					continue;
				
				Dungeon dungeon = new Dungeon(rail, this);
				dungeons.Add(dungeon);
				rail_map[rail] = dungeon;
			}
		}
		
		finalize();
		
		foreach (var dungeon in dungeons) {
			dungeon.generate_code1();
			target.generate_rail_code(dungeon);
			dungeon.generate_code2();
		}
	}
	
	void finalize () {
		foreach (var dungeon in dungeons) {
			dungeon.rail.finalize();
		}
	}
	
	void post_analyze () {
		foreach (var dungeon in dungeons) {
			dungeon.post_analyze_many(dungeon.code);
		}
	}

	public void flatten () {
		foreach (var dungeon in dungeons) {
			dungeon.flatten();
		}
	}
	
	public Dungeon get_dungeon (Rail rail) {
		return rail_map[rail];
	}

	public void process (Node expression, Scope scope) {
		switch(expression.type) {
			case Node_Type.scope:
                scope_expression((metahub.meta.types.Scope_Expression)expression, scope);
		        break;

			case Node_Type.block:
				block_expression((metahub.meta.types.Block) expression, scope);
		        break;

			case Node_Type.constraint:
				create_constraint((metahub.meta.types.Constraint)expression, scope);
		        break;

			case Node_Type.function_scope:
                function_scope((metahub.meta.types.Function_Scope)expression, scope);
		        break;

			case metahub.meta.types.Node_Type.path:
                block_expression((metahub.meta.types.Block)expression, scope);
		        break;
				
			case Node_Type.property:
			case Node_Type.function_call:
		        break;
				
			default:
				throw new Exception("Cannot process Node of type :" + expression.type + ".");
		}
	}

    void scope_expression(metahub.meta.types.Scope_Expression expression, Scope scope)
    {
		//Scope new_scope = new Scope(scope.hub, Node.scope_definition, scope);
		foreach (var child in expression.children) {
			process(child, expression.scope);
		}
	}

	void block_expression (metahub.meta.types.Block expression, Scope scope) {
		foreach (var child in expression.children) {
			process(child, scope);
		}
	}

    void function_scope(metahub.meta.types.Function_Scope expression, Scope scope)
    {
		process(expression.expression, scope);
		foreach (var child in expression.lambda.expressions) {
			process(child, expression.lambda.scope);
		}
	}
	
	void create_constraint (metahub.meta.types.Constraint expression, Scope scope) {
		var rail = scope.rail;
		metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(expression, this);
		var tie = Parse.get_end_tie(constraint.reference);
		//trace("tie", tie.rail.name + "." + tie.name);
		tie.constraints.Add(constraint);
		constraints.Add(constraint);
	}
	
	//Node create_lambda_constraint (metahub Node.meta.types.Constraint, Scope scope) {
		//throw "";
		//var rail = get_rail(scope.trellis);
		//metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(Node, this);
		//var tie = Parse.get_end_tie(constraint.reference);
		//trace("tie", tie.rail.name + "." + tie.name);
		//tie.constraints.Add(constraint);
		//constraints.Add(constraint);
		//return null;
	//}

	public Rail get_rail (Trellis trellis) {
		return railway.get_rail(trellis);
	}

	public void implement_constraint (Constraint constraint) {
		var tie = Parse.get_end_tie(constraint.reference);

		if (tie.type == Kind.list) {
			List_Code.generate_constraint(constraint, this);
		}
		else {
			var dungeon = get_dungeon(tie.rail);
			dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, this));
		}
	}

    public Expression translate(metahub.meta.types.Node expression, Scope scope = null)
    {
		switch(expression.type) {
			case Node_Type.literal:
                return new Literal(((metahub.meta.types.Literal_Value)expression).value, new Signature(Kind.unknown));

			case Node_Type.function_call:
                //metahub.meta.types.Function_Call func = expression;
				//return new Function_Call(func.name, [translate(func.input)]);
				throw new Exception("Not implemented.");

			case Node_Type.path:
                return convert_path((metahub.meta.types.Reference_Path)expression);

			case Node_Type.block:
                return new Create_Array(((metahub.meta.types.Block)expression).children.Select((e) => translate(e)));
				
			case Node_Type.lambda:
				
				return null;
				//metahub.meta.types.Lambda lambda = Node;
				//return new Anonymous_(lambda.parameters.map(function(p)=> new Parameter(p.name, p.signature)),
					//lambda.expressions.map((e)=> translate(e, lambda.scope))
				//);
//
			//case metahub.meta.types.Expression_Type.constraint:
				//return create_lambda_constraint(Node, scope);
			
			default:
				throw new Exception("Cannot convert Node " + expression.type + ".");
		}
	}

    public Expression convert_path(metahub.meta.types.Reference_Path expression)
    {
		var path = expression.children;
		List<Expression> result = new List<Expression>();
		var first = (metahub.meta.types.Property_Reference)path[0];
		Rail rail = first.tie.get_abstract_rail();
		foreach (var token in path) {
			if (token.type == metahub.meta.types.Node_Type.property) {
                var property_token = (metahub.meta.types.Property_Reference)token;
				var tie = rail.all_ties[property_token.tie.name];
				if (tie == null)
					throw new Exception("tie is null: " + property_token.tie.fullname());

                result.Add(new Property_Expression(tie));
				rail = tie.other_rail;
			}
			else {
				var function_token = (metahub.meta.types.Function_Call)token;
                result.Add(new Function_Call(function_token.name, new List<Expression>(), true));
			}
		}
		return new metahub.imperative.types.Path(result);
	}
		
}}