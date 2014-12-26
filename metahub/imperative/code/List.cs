using metahub.imperative.Imp;
using metahub.logic.schema.Constraint;
using metahub.logic.schema.Rail;
using metahub.logic.schema.Tie;
using metahub.imperative.types.*;
using metahub.meta.types.Lambda;
using metahub.schema.Kind;

namespace e {
/**
 * ...
 * @author Christopher W. Johnson
 */
class List
{
	public static void common_functions (Tie tie, Imp imp) {
		var rail = tie.rail;
		var dungeon = imp.get_dungeon(tie.rail);

		var function_name = tie.tie_name + "_add";
		Function_Definition definition = new Function_Definition(function_name, dungeon, [
			new Parameter("item", tie.get_other_signature()),
			new Parameter("origin", { type: Kind.reference, rail: null })
			], []);

		var zone = dungeon.create_zone(definition.expressions);
		var mid = zone.divide(null, [
			new Property_Expression(tie,
				new Function_Call("add", [ new Variable("item") ], true)
			)
		]);
		var post = zone.divide(function_name + "_post");

		if (tie.other_tie != null) {
			//throw "";
			mid.Add(
				new Assignment(new Variable("item", new Property_Expression(tie.other_tie)),
				"=", new Self())
			);
		}

		dungeon.add_to_block("/", definition);
	}

	public static void generate_constraint (Constraint constraint, Imp imp) {
		Path path = cast constraint.reference;
		Property_Expression property_expression = cast path.children[0];
		var reference = property_expression.tie;
		//int amount = target.render_expression(constraint.expression, constraint.scope);
		var expression = constraint.expression;

		//if (constraint.expression.type == metahub.meta.types.Expression_Type.function_call) {
			//metahub.meta.types.Function_Call func = cast constraint.expression;
			//if (func.name == "map") {
				//map(constraint, expression, imp);
				//return;
			//}
		//}

		var other_path = Parse.get_path(expression);
		if (other_path.Count() > 0 && other_path[other_path.Count() - 1].type == Kind.list) {
			map(constraint, expression, imp);
		}
		else {
			size(constraint, expression, imp);
		}
	}

	public static void map (Constraint constraint, metahub expression.meta.types.Expression, Imp imp) {
		var start = Parse.get_start_tie(constraint.reference);
		var end = Parse.get_end_tie(constraint.reference);
		metahub.meta.types.Path path = cast constraint.expression;

		var a = Parse.get_path(constraint.reference);
		var b = Parse.get_path(path);

		link(a, b, Parse.reverse_path(b.slice(0, a.Count() - 1)), constraint.lambda, imp);
		link(b, a, a.slice(0, a.Count() - 1), constraint.lambda, imp);
	}

	public static void link (List<Tie> a, List<Tie> b, List<Tie> c, Lambda mapping, Imp imp) {
		var a_start = a[0];
		var a_end = a[a.Count() - 1];

		var second_start = b[0];
		var second_end = b[b.Count() - 1];

		var item_name = second_end.rail.name.toLowerCase() + "_item";

		List<Expression> creation_block = [
			new Declare_Variable(item_name, second_end.get_other_signature(), new Instantiate(second_end.other_rail)),
		];

		if (mapping != null) {
			List<metahub.meta.types.Constraint> constraints = cast mapping.expressions;
			foreach (var constraint in constraints) {
				metahub.meta.types.Path first = cast constraint.first;
				metahub.meta.types.Property_Expression first_tie = cast first.children[1];
				metahub.meta.types.Path second = cast constraint.second;
				metahub.meta.types.Property_Expression second_tie = cast second;
				creation_block.Add(new Assignment(
					new Variable(item_name, new Property_Expression(cast a_end.other_rail.get_tie_or_error(first_tie.tie.name))),
					"=",
					new Variable("item", new Property_Expression(cast second_end.other_rail.get_tie_or_error(second_tie.tie.name)))
				));
			}
		}

		creation_block = creation_block.concat(cast [
			new Variable(item_name, new Function_Call("initialize")),
			new Property_Expression(c[0],
				new Function_Call(second_end.tie_name + "_add",
					[new Variable(item_name), new Self()])
			)
		]);

		List<Expression> block = [
				new Flow_Control("if", new Condition("!=", [
				new Variable("origin"), new Property_Expression(c[0])]), creation_block)
		];

		if (a_start.other_tie.property.allow_null) {
			block = [
				new Flow_Control("if",
					new Condition("!=", [ new Property_Expression(a_start.other_tie),
					new Null_Value() ]), block
				)
			];
		}
		imp.get_dungeon(a_end.rail).concat_block(a_end.tie_name + "_add_post", block);
	}

	public static void size (Constraint constraint, metahub expression.meta.types.Expression, Imp imp) {
		Path path = cast constraint.reference;
		Property_Expression property_expression = cast path.children[0];
		var reference = property_expression.tie;

		var instance_name = reference.other_rail.rail_name;
		var rail = reference.other_rail;
		Rail local_rail = reference.rail;
		var child = "child";
		Flow_Control flow_control = new Flow_Control("while", new Condition("<",
			[
				imp.translate(constraint.reference),
				//{ type: "path", path: constraint.reference },
				imp.translate(expression)
			]),[
			new Declare_Variable(child, {
					type: Kind.reference,
					rail: rail,
					is_value: false
			}, new Instantiate(rail)),
			new Variable(child, new Function_Call("initialize")),
			new Function_Call(reference.tie_name + "_add",
				[new Variable(child), new Null_Value()])
	]);
		imp.get_dungeon(local_rail).add_to_block("initialize", flow_control);
	}

}}