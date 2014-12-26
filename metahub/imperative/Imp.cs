using System.Collections.Generic;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.meta;
using metahub.meta.types;
using metahub.render;
using Constraint = metahub.logic.schema.Constraint;

namespace metahub.imperative {
/**
 * ...
 * @author Christopher W. Johnson
 */
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

	public void run (metahub.meta.types.Expression root, Target target) {
		process(root, null);
		generate_code(target);

		foreach (var constraint in constraints) {
			implement_constraint(constraint);
		}

		flatten();
		
		post_analyze();
	}

	public void generate_code (Target target) {
		
		foreach (var region in railway.regions) {
			if (region.is_external)
				continue;

			foreach (var rail in region.rails) {
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

	public void process (metahub expression.meta.types.Expression, Scope scope) {
		switch(expression.type) {
			case metahub.meta.types.Expression_Type.scope:
				scope_expression(expression, scope);

			case metahub.meta.types.Expression_Type.block:
				block_expression(expression, scope);

			case metahub.meta.types.Expression_Type.constraint:
				create_constraint(expression, scope);

			case metahub.meta.types.Expression_Type.function_scope:
				function_scope(expression, scope);

			case metahub.meta.types.Expression_Type.path:
				block_expression(expression, scope);
				
			case metahub.meta.types.Expression_Type.property:
			case metahub.meta.types.Expression_Type.function_call:
				
			default:
				throw new Exception("Cannot process expression of type :" + expression.type + ".");
		}
	}

	void scope_expression (Scope_Expression expression, Scope scope) {
		//Scope new_scope = new Scope(scope.hub, expression.scope_definition, scope);
		foreach (var child in expression.children) {
			process(child, expression.scope);
		}
	}

	void block_expression (metahub expression.meta.types.Block, Scope scope) {
		foreach (var child in expression.children) {
			process(child, scope);
		}
	}

	void function_scope (Function_Scope expression, Scope scope) {
		process(expression.expression, scope);
		foreach (var child in expression.lambda.expressions) {
			process(child, expression.lambda.scope);
		}
	}
	
	void create_constraint (metahub.meta.types.Constraint expression, Scope scope) {
		Rail rail = scope.rail;
		metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(expression, this);
		var tie = Parse.get_end_tie(constraint.reference);
		//trace("tie", tie.rail.name + "." + tie.name);
		tie.constraints.Add(constraint);
		constraints.Add(constraint);
		trace("constraint", constraint.op, tie.fullname());
	}
	
	//Expression create_lambda_constraint (metahub expression.meta.types.Constraint, Scope scope) {
		//throw "";
		//var rail = get_rail(scope.trellis);
		//metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(expression, this);
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
			List.generate_constraint(constraint, this);
		}
		else {
			var dungeon = get_dungeon(tie.rail);
			dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, this));
		}
	}

	public Expression translate (metahub expression.meta.types.Expression, Scope scope = null) {
		switch(expression.type) {
			case metahub.meta.types.Expression_Type.literal:
				metahub.meta.types.Literal literal = expression;
				return new Literal(literal.value, { Kind type.unknown });

			case metahub.meta.types.Expression_Type.function_call:
				metahub.meta.types.Function_Call func = expression;
				//return new Function_Call(func.name, [translate(func.input)]);
				throw new Exception("Not implemented.");

			case metahub.meta.types.Expression_Type.path:
				return convert_path(expression);

			case metahub.meta.types.Expression_Type.block:
				metahub.meta.types.Block array = expression;
				return new Create_Array(array.children.map((e)=> translate(e)));
				
			case metahub.meta.types.Expression_Type.lambda:
				
				return null;
				//metahub.meta.types.Lambda lambda = expression;
				//return new Anonymous_(lambda.parameters.map(function(p)=> new Parameter(p.name, p.signature)),
					//lambda.expressions.map((e)=> translate(e, lambda.scope))
				//);
//
			//case metahub.meta.types.Expression_Type.constraint:
				//return create_lambda_constraint(expression, scope);
			
			default:
				throw new Exception("Cannot convert expression " + expression.type + ".");
		}
	}

	public Expression convert_path (metahub expression.meta.types.Path) {
		var path = expression.children;
		List<metahub.imperative.types.Expression> result = new List<metahub.imperative.types.Expression>();
		metahub.meta.types.Property_Expression first = path[0];
		Rail rail = first.tie.get_abstract_rail();
		foreach (var token in path) {
			if (token.type == metahub.meta.types.Expression_Type.property) {
				metahub.meta.types.Property_Expression property_token = token;
				var tie = rail.all_ties[property_token.tie.name];
				if (tie == null)
					throw new Exception("tie is null: " + property_token.tie.fullname());

				result.Add(new Property_Expression(tie));
				rail = tie.other_rail;
			}
			else {
				metahub.meta.types.Function_Call function_token = token;
				result.Add(new Function_Call(function_token.name, [], true));
			}
		}
		return new metahub.imperative.types.Path(result);
	}
		
}}