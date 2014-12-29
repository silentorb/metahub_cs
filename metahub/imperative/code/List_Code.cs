using System.Collections.Generic;
using System.Linq;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.meta;
using metahub.meta.types;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Function_Call;
using Parameter = metahub.imperative.types.Parameter;
using Path = metahub.imperative.types.Path;
using Variable = metahub.imperative.types.Variable;

namespace metahub.imperative.code {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class List_Code
{
	public static void common_functions (Tie tie, Imp imp) {
		var rail = tie.rail;
		var dungeon = imp.get_dungeon(tie.rail);

		var function_name = tie.tie_name + "_add";
		Function_Definition definition = new Function_Definition(function_name, dungeon, new List<Parameter> {
			new Parameter("item", tie.get_other_signature()),
			new Parameter("origin", new Signature { type = Kind.reference, rail = null })
        }, new List<Expression>());

		var zone = dungeon.create_zone(definition.expressions);
		var mid = zone.divide(null, new List<Expression> {
			new Property_Expression(tie,
				new Function_Call("add",new List<Expression>{ new Variable("item") }, true)
			)
		});
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
		var path = (Reference_Path)constraint.reference;
        var property_expression = (Property_Reference)path.children[0];
		var reference = property_expression.tie;
		//int amount = target.render_expression(constraint.Node, constraint.scope);
		var expression = constraint.expression;

		//if (constraint.Node.type == metahub.meta.types.Expression_Type.function_call) {
			//metahub.meta.types.Function_Call func = constraint.Node;
			//if (func.name == "map") {
				//map(constraint, Node, imp);
				//return;
			//}
		//}

		var other_path = Parse.get_path(expression);
		if (other_path.Count > 0 && other_path[other_path.Count - 1].type == Kind.list) {
			map(constraint, expression, imp);
		}
		else {
			size(constraint, expression, imp);
		}
	}

	public static void map (Constraint constraint, Node expression, Imp imp) {
		var start = Parse.get_path(constraint.reference).First();
		var end = Parse.get_end_tie(constraint.reference);
		var path = (Property_Reference)constraint.expression;

		var a = Parse.get_path(constraint.reference);
		var b = Parse.get_path(path);

		link(a, b, Parse.reverse_path(b.Take(a.Count - 1)), constraint.lambda, imp);
		link(b, a, a.Take(a.Count - 1), constraint.lambda, imp);
	}

	public static void link (List<Tie> a, List<Tie> b, IEnumerable<Tie> c, Lambda mapping, Imp imp) {
		var a_start = a[0];
		var a_end = a[a.Count - 1];

		var second_start = b[0];
		var second_end = b[b.Count - 1];

		var item_name = second_end.rail.name.ToLower() + "_item";

		var creation_block = new List<Expression>();
	    creation_block.Add(new Declare_Variable(item_name, second_end.get_other_signature(),
	                                             new Instantiate(second_end.other_rail)));

		if (mapping != null) {
			foreach (metahub.meta.types.Constraint constraint in mapping.expressions) {
				var first = constraint.first as Reference_Path;
				var first_tie = first.children[1] as Property_Reference;
				var second = constraint.second as Property_Reference;
                //var second_tie = second.children[] as Property_Reference;
				creation_block.Add(new Assignment(
					new Variable(item_name, new Property_Expression(a_end.other_rail.get_tie_or_error(first_tie.tie.name))),
					"=",
					new Variable("item", new Property_Expression(second_end.other_rail.get_tie_or_error(second.tie.name)))
				));
			}
		}

		creation_block = creation_block.Union(new List<Expression>{
			new Variable(item_name, new Function_Call("initialize")),
			new Property_Expression(c.First(),
				new Function_Call(second_end.tie_name + "_add",
				new List<Expression> { new Variable(item_name), new Self()}))
		}).ToList();

		List<Expression> block = new List<Expression> {
				new Flow_Control("if", new Condition("!=", new List<Expression> {
				new Variable("origin"), new Property_Expression(c.First())}), creation_block)
		};

		if (a_start.other_tie.property.allow_null) {
			block = new List<Expression> {
				new Flow_Control("if",
					new Condition("!=", new List<Expression> { new Property_Expression(a_start.other_tie),
					new Null_Value() }), block
				)
			};
		}
		imp.get_dungeon(a_end.rail).concat_block(a_end.tie_name + "_add_post", block);
	}

	public static void size (Constraint constraint, Node expression, Imp imp) {
		var path = (Reference_Path) constraint.reference;
		var property_expression = (Property_Reference)path.children[0];
		var reference = property_expression.tie;

		var instance_name = reference.other_rail.rail_name;
		var rail = reference.other_rail;
		Rail local_rail = reference.rail;
		const string child = "child";
        var scope = new metahub.meta.Scope();
		Flow_Control flow_control = new Flow_Control("while", new Condition("<",
		new List<Expression>{
				imp.translate(constraint.reference, scope),
				//{ type: "path", path: constraint.reference },
				imp.translate(expression, scope)
        }),new List<Expression> {
			new Declare_Variable(child,new Signature {
					type = Kind.reference,
					rail = rail,
					is_value = false
			}, new Instantiate(rail)),
			new Variable(child, new Function_Call("initialize")),
			new Function_Call(reference.tie_name + "_add",
			new List<Expression> {new Variable(child), new Null_Value()})
	});
	    var dungeon = imp.get_dungeon(local_rail);
		dungeon.add_to_block("initialize", flow_control);
	}

}}