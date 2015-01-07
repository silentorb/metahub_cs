using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.logic.types;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Function_Call;
using Parameter = metahub.imperative.types.Parameter;
using Variable = metahub.imperative.types.Variable;

namespace metahub.imperative.code
{
    public class List_Code
    {
        public static void common_functions(Tie tie, Overlord imp, Scope scope)
        {
            var rail = tie.rail;
            var dungeon = imp.get_dungeon(tie.rail);
            var root_block = dungeon.get_block("class_definition");

            var function_name = "add_" + tie.tie_name;
            var function_scope = new Scope(scope);
            var item = function_scope.create_symbol("item", tie.get_other_signature());
            var origin = function_scope.create_symbol("origin", new Signature(Kind.reference));
            Function_Definition definition = new Function_Definition(function_name, dungeon, new List<Parameter> {
			    new Parameter(item),
			    new Parameter(origin, new Null_Value())
            }, new List<Expression>());

            var block = dungeon.create_block(function_name, scope, definition.expressions);
            var mid = block.divide(null, new List<Expression> {
			new Tie_Expression(tie,
				new Function_Call("add",new Expression[]{ new Variable(item) }, true)
			)
		});
            var post = block.divide("post");

            if (tie.other_tie != null)
            {
                mid.add(
                    new Flow_Control(Flow_Control_Type.If, new Operation("!=", new List<Expression>
                {
                    new Variable(origin), new Variable(item)
                }), new List<Expression> {
                    new Variable(item, 
                        new Property_Function_Call(Property_Function_Type.set,  tie.other_tie,
                            new List<Expression>
                                {
                                    new Self(dungeon),
                                    new Self(dungeon)
                                }))
                }));
            }
            
            root_block.add(definition);
        }

        public static void generate_constraint(Constraint constraint, Overlord imp)
        {
            var path = constraint.first;
            var property_expression = (Property_Reference)path[0];
            var reference = property_expression.tie;
            //int amount = target.render_expression(constraint.Node, constraint.scope);
            var expression = constraint.second;

            //if (constraint.Node.type == metahub.logic.types.Expression_Type.function_call) {
            //metahub.logic.types.Function_Call func = constraint.Node;
            //if (func.name == "map") {
            //map(constraint, Node, imp);
            //return;
            //}
            //}

            var other_path = expression;
            if (other_path.Length > 0 && other_path[other_path.Length - 1].get_signature().type == Kind.list)
            {
                map(constraint, expression, imp);
            }
            else
            {
                size(constraint, expression, imp);
            }
        }

        public static void map(Constraint constraint, Node[] expression, Overlord imp)
        {
            var start = constraint.first.First();
            var end = constraint.first;
            var path = constraint.second;

            var a = constraint.first.Select(i => ((Property_Reference)i).tie).ToList();
            var b = path.Select(i => ((Property_Reference)i).tie).ToList();

            link(a, b, Parse.reverse_path(b.Take(a.Count - 1)), constraint.lambda, imp);
            link(b, a, a.Take(a.Count - 1), constraint.lambda, imp);
        }

        public static void link(List<Tie> a, List<Tie> b, IEnumerable<Tie> c, Lambda mapping, Overlord imp)
        {
            var a_start = a[0];
            var a_end = a[a.Count - 1];

            var second_start = b[0];
            var second_end = b[b.Count - 1];

            var item_name = second_end.rail.name.ToLower() + "_item";

            var function_block = imp.get_dungeon(a_end.rail).get_block("add_" + a_end.tie_name);
            var new_scope = new Scope(function_block.scope);
            var item = new_scope.create_symbol("item", second_end.get_other_signature());
            var item2 = new_scope.create_symbol("item", second_end.get_other_signature());
            var origin = new_scope.create_symbol("origin", second_end.get_other_signature());
            var creation_block = new List<Expression>();
            creation_block.Add(new Declare_Variable(item2,
                new Instantiate(second_end.other_rail))
            );

            if (mapping != null)
            {
                foreach (Constraint_Wrapper wrapper in mapping.expressions)
                {
                    var constraint = wrapper.constraint;
                    var first = constraint.first;
                    var first_tie = a_end.other_rail.get_tie_or_error(((Property_Reference) first[1]).tie.name);
                    var second = (Property_Reference)Overlord.simplify_path(constraint.second)[0];
                    //var second_tie = second.children[] as Property_Reference;
                    creation_block.Add(new Variable(item2, new Function_Call("set_" + first_tie.name, new Expression[]
                        {
                        new Variable(item, new Tie_Expression(second_end.other_rail.get_tie_or_error(second.tie.name)))
                        }
                       )
                    ));
                }
            }

            var dungeon = imp.get_dungeon(second_end.rail);
            creation_block = creation_block.Union(new List<Expression>{
			new Variable(item2, new Function_Call("initialize")),
			new Tie_Expression(c.First(),
				new Function_Call("add_" + second_end.tie_name,
				new Expression[] { new Variable(item2), new Self(dungeon)}))
		}).ToList();

            List<Expression> block = new List<Expression> {
				new Flow_Control(Flow_Control_Type.If, new Operation("!=", new List<Expression> {
				new Variable(origin), new Tie_Expression(c.First())}), creation_block)
		};

            if (a_start.other_tie.property.allow_null)
            {
                block = new List<Expression> {
				new Flow_Control(Flow_Control_Type.If, 
					new Operation("!=", new List<Expression> { new Tie_Expression(a_start.other_tie),
					new Null_Value() }), block
				)
			};
            }
            function_block.add_many("post", block);
        }

        public static void size(Constraint constraint, Node[] expression, Overlord imp)
        {
            var path = constraint.first;
            var property_expression = (Property_Reference)path[0];
            var reference = property_expression.tie;

            var instance_name = reference.other_rail.rail_name;
            var rail = reference.other_rail;
            Rail local_rail = reference.rail;
            var dungeon = imp.get_dungeon(local_rail);
            var block = dungeon.get_block("initialize");

            //const string child = "child";
            var new_scope = new Scope(block.scope);
            var child = new_scope.create_symbol("child", new Signature(Kind.reference, rail));
            var logic_scope = new Scope();
            var imp_ref = (Tie_Expression)imp.convert_path(Overlord.simplify_path(constraint.first), logic_scope);
            imp_ref.child = new Function_Call("count", null, true);
            Flow_Control flow_control = new Flow_Control(Flow_Control_Type.While, new Operation("<",
            new List<Expression>{
				imp_ref,
				//{ type: "path", path: constraint.reference },
				imp.convert_path(expression, logic_scope)
        }), new List<Expression> {
			new Declare_Variable(child, new Instantiate(rail)),
			new Variable(child, new Function_Call("initialize")),
			new Function_Call("add_" + reference.tie_name,
			new List<Expression> { new Variable(child), new Null_Value() })
	});

            block.add(flow_control);
        }

    }
}