using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
//using metahub.logic.types;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Literal_Value = metahub.logic.types.Literal_Value;
using Node_Type = metahub.logic.types.Node_Type;

namespace metahub.imperative.code
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
    public class Reference
    {
        public static Dictionary<string, string> inverse_operators = new Dictionary<string, string>
        {
            {">", "<="},
            {"<", ">="},
            {">=", "<"},
            {"<=", ">"}
        };

        public static List<Expression> constraint(Constraint constraint, Tie tie, Imp imp, Scope scope)
        {
            var op = constraint.op;
            //return new List<Expression>();
            var first_last = constraint.first.Last();
            if (first_last.type == Node_Type.function_call
                && ((metahub.logic.types.Function_Call)first_last).name == "dist")
            {
                return cross(constraint, tie, imp, scope);
            }
            else
            {
                var reference = imp.convert_path(constraint.first, null);

                //if (constraint.first.)

                if (op == "in")
                {
                    var args = ((metahub.logic.types.Array_Expression)constraint.second[0]).children;
                    return generate_constraint(reference, ">=", (metahub.logic.types.Literal_Value)args[0])
                        .Union(
                            generate_constraint(reference, "<=", (metahub.logic.types.Literal_Value)args[1])
                        ).ToList();
                }

                return generate_constraint(reference, constraint.op, (Literal_Value)constraint.second.First());
            }
        }

        static List<Expression> generate_constraint(Expression reference, string op, Literal_Value literal)
        {
            var inverse = inverse_operators[op];
            float limit = literal.get_float();

            const float min = 0.0001f;
            float value = 0;
            switch (op)
            {
                case "<":
                    value = limit - min;
                    break;

                case ">":
                    value = limit + min;
                    break;

                case "<=":
                    value = limit;
                    break;

                case ">=":
                    value = limit;
                    break;
            }

            return new List<Expression> { new Flow_Control(Flow_Control_Type.If, new Operation(inverse,
				new List<Expression> {
					reference,
				new Literal(limit, new Signature(Kind.Float))
            }),
		        new List<Expression>{
				    new Assignment(reference, "=", new Literal(value, new Signature { type = Kind.Float }))
                }
		    )};
        }

        public static List<Expression> cross(Constraint constraint, Tie tie, Imp imp, Scope scope)
        {
            //var dungeon = imp.get_dungeon(tie.rail);
            //dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, this));
            var property_reference = (metahub.logic.types.Property_Reference)constraint.caller.First();
            var dungeon = imp.get_dungeon(tie.rail);

            var iterator_scope = new Scope(scope);
            var it = iterator_scope.create_symbol("item", new Signature(Kind.reference, property_reference.tie.other_rail));
            iterator_scope.add_map("a", () => new Variable(it));
            iterator_scope.add_map("b", () => new Self(dungeon));

            var class_block = dungeon.get_block("class_definition");
            var function_scope = new Scope(class_block.scope);
            var value = function_scope.create_symbol("value", tie.get_signature());
            var function_name = "check_" + property_reference.tie.tie_name + "_" + property_reference.tie.other_tie.tie_name;
            class_block.add(new Function_Definition(function_name, dungeon, new List<Parameter>{
				new Parameter(value)
			}, new List<Expression>
			    {
			        new Flow_Control(Flow_Control_Type.If, new Operation("==", new List<Expression>
			            {
			                new Property_Expression(property_reference.tie.other_tie),
                            new Null_Value()
			            }), new List<Expression>{ 
                            new Statement("return", new Literal(true, new Signature(Kind.Bool)))
                        }),
                    new Iterator(it, 
                        new Property_Expression(property_reference.tie.other_tie, 
                            new Property_Expression(property_reference.tie)), 
                        new List<Expression>
                        {
                            new Flow_Control(Flow_Control_Type.If, new Operation("==", new List<Expression>
                                {
                                    new Variable(it), new Self(dungeon)
                                }), new List<Expression>
                                    {
                                        new Statement("continue")
                                    })
                                    
                        }.Concat(dist(constraint, tie, imp, iterator_scope, it, value))
                        .ToList()),
                    new Statement("return", new Literal(true, new Signature(Kind.Bool)))
			    }, new Signature(Kind.Bool))
            );

            var setter_block = dungeon.get_block("set_" + property_reference.tie.other_tie.tie_name);
            setter_block.add("post", new Function_Call(function_name, new List<Expression>
                {
                    new Property_Expression(constraint.endpoints.First())
                })
            );

            //return new List<Expression>();
            return new List<Expression>
                {
                    new Flow_Control(Flow_Control_Type.If, 
                    new Operation("==", new List<Expression>{ 
                    new Function_Call(function_name, new List<Expression>
                        {
                            new Variable(scope.find("value"))
                        }),
                    new Literal(false, new Signature(Kind.Bool))
                    }), new List<Expression>
                            {
                                new Statement("return")
                            })
                };

        }

        public static List<Expression> dist(Constraint constraint, Tie tie, Imp imp, Scope scope, Symbol it, Symbol value)
        {
            var offset = scope.create_symbol("offset", value.signature);
            return new List<Expression>
            {
                new Flow_Control(Flow_Control_Type.If,
                    new Operation(inverse_operators[constraint.op], new List<Expression>
                    {
                        new Variable(it, new Property_Expression(constraint.endpoints.Last(),
                            new Function_Call("dist",
                                new List<Expression>
                                {
                                    new Variable(value)
                                }, true))),
                        imp.translate(constraint.second.First(), scope)
                    }),
                    new List<Expression>
                    {
                        new Declare_Variable(offset,new Operation("/", new List<Expression>{ new Operation("+", new List<Expression>
                        {
                            new Variable(it, new Property_Expression(constraint.endpoints.Last())),
                            new Variable(value)
                        }), new Literal(2, new Signature(Kind.Float))})),
                            new Variable(it, new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
                                { new Operation("+", new List<Expression> {
                                    new Variable(it, new Property_Expression(constraint.endpoints.Last())),
                                    new Variable(offset)}) })),
                            new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
                                { new Operation("+", new List<Expression> {
                                    new Variable(value),
                                    new Variable(offset)}) }),
                            new Statement("return", new Literal(false, new Signature(Kind.Bool)))

                    })
            };
        }

    }
}