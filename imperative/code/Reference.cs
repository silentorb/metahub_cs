using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.imperative.summoner;
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

        public static void generate_constraint(Constraint constraint, Tie tie, Overlord overlord)
        {
            if (constraint.op == "!="
                        && constraint.second.Length == 1 && constraint.second[0].type == Node_Type.Null)
            {
                not_null((Tie_Expression)overlord.convert_path(constraint.first, null), overlord);
            }
            else if (Constraint.self_modifying_operators.Contains(constraint.op))
            {
                self_modifying(constraint, tie, overlord);
            }
            else
            {
                var dungeon = overlord.get_dungeon(tie.rail);
                var block = dungeon.get_block("set_" + tie.tie_name);
                block.add_many("pre", Reference.constraint(constraint, tie, overlord, block.scope));
            }
        }

        public static void self_modifying(Constraint constraint, Tie tie, Overlord overlord)
        {
            var property_node = (metahub.logic.types.Property_Reference)constraint.first.First();
            var dungeon = overlord.get_dungeon(property_node.tie.rail);
            var imp = dungeon.summon_imp("tick");
            imp.expressions.Add(new Assignment(
                overlord.convert_path(constraint.first, null),
                constraint.op,
                overlord.convert_path(constraint.second, null)
                ));
        }

        public static List<Expression> constraint(Constraint constraint, Tie tie, Overlord overlord, Scope scope)
        {
            var op = constraint.op;
            //return new List<Expression>();
            var first_last = constraint.first.Last();
            if (first_last.type == Node_Type.function_call
                && ((metahub.logic.types.Function_Call)first_last).name == "dist")
            {
                return cross(constraint, tie, overlord, scope);
            }
            else
            {
                var reference = overlord.convert_path(constraint.first, scope);

                //if (constraint.first.)

                if (op == ">=<")
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

        public static void not_null(Tie_Expression reference, Overlord overlord)
        {
            var tie = reference.tie;
            var dungeon = overlord.get_dungeon(tie.rail);
            var other_dungeon = overlord.get_dungeon(tie.other_rail);
            var portal = dungeon.all_portals[tie.tie_name];
            var block = dungeon.get_block("initialize");

            block.add_many("pre", new List<Expression>
                {
                  new Assignment(new Portal_Expression(portal), "=", new Instantiate(other_dungeon)),
                  new Function_Call("initialize", new Portal_Expression(portal)),
                });
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
				new Literal(limit)
            }),
		        new List<Expression>{
				    new Assignment(reference, "=", new Literal(value))
                }
		    )};
        }

        public static List<Expression> cross(Constraint constraint, Tie tie, Overlord overlord, Scope scope)
        {
            //var dungeon = imp.get_dungeon(tie.rail);
            //dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, this));
            var property_reference = (metahub.logic.types.Property_Reference)constraint.constraint_scope.caller.First();
            var dungeon = overlord.get_dungeon(tie.rail);

            var iterator_scope = new Scope(scope);
            var it = iterator_scope.create_symbol("item", new Signature(Kind.reference, property_reference.tie.other_rail));
            iterator_scope.add_map("a", () => new Variable(it));
            iterator_scope.add_map("b", () => new Self(dungeon));

            var class_block = dungeon.get_block("class_definition");
            var function_scope = new Scope(class_block.scope);
            var value = function_scope.create_symbol("value", tie.get_signature());
            var function_name = "check_" + property_reference.tie.tie_name + "_" + property_reference.tie.other_tie.tie_name;
            var imp = dungeon.spawn_imp(function_name, new List<Parameter>
                {
                    new Parameter(value)
                }, new List<Expression>
                    {
                        Imp.If(new Operation("==", new List<Expression>
                            {
                                new Tie_Expression(property_reference.tie.other_tie),
                                new Null_Value()
                            }), new List<Expression>
                                {
                                    new Statement("return", Imp.True())
                                }),
                        new Iterator(it,
                                     new Tie_Expression(property_reference.tie.other_tie,
                                                        new Tie_Expression(property_reference.tie)),
                                     new List<Expression>
                                         {
                                             Imp.If(new Operation("==", new List<Expression>
                                                 {
                                                     new Variable(it),
                                                     new Self(dungeon)
                                                 }), new List<Expression>
                                                     {
                                                         new Statement("continue")
                                                     })

                                         }.Concat(dist(constraint, tie, overlord, iterator_scope, it, value))
                                          .ToList()),
                        new Statement("return", new Literal(true))
                    }, new Signature(Kind.Bool));

            var setter_block = dungeon.get_block("set_" + property_reference.tie.other_tie.tie_name);
            setter_block.add("post", new Function_Call(function_name, null, new Expression[]
                {
                    new Tie_Expression(constraint.endpoints.First())
                })
            );

            //return new List<Expression>();
            return new List<Expression>
                {
                    new Flow_Control(Flow_Control_Type.If, 
                    new Operation("==", new List<Expression>{ 
                    new Function_Call(function_name, null, new Expression[]
                        {
                            new Variable(scope.find_or_exception("value"))
                        }),
                    new Literal(false)
                    }), new List<Expression>
                            {
                                new Statement("return")
                            })
                };

        }

        public static List<Expression> dist(Constraint constraint, Tie tie, Overlord overlord, Scope scope, Symbol it, Symbol value)
        {
            var offset = scope.create_symbol("offset", value.signature);
            var dungeon = overlord.get_dungeon(tie.rail);
            var conflict_dungeon = create_conflict_class(constraint, tie.rail, overlord);
            var conflict_nodes = conflict_dungeon.all_portals["nodes"];
            var conflict = scope.create_symbol("conflict", new Profession(Kind.reference, conflict_dungeon));
            var mold_tie = tie.rail.get_tie_or_error("mold");
            var piecemaker_tie = mold_tie.other_rail.get_tie_or_error("piece_maker");
            var conflicts_tie = piecemaker_tie.other_rail.get_tie_or_error("conflicts");

            return new List<Expression>
            {
                Imp.If(Imp.operation(inverse_operators[constraint.op], 
                        new Variable(it, new Tie_Expression(constraint.endpoints.Last(),
                            new Function_Call("dist", null,
                                new List<Expression> { new Variable(value) }, true))),
                        overlord.translate(constraint.second.First(), scope)
                    ),
                    new List<Expression>
                    {
                        new Declare_Variable(offset, Imp.operation("/", Imp.operation("+",
                            new Variable(it, new Tie_Expression(constraint.endpoints.Last())),
                            new Variable(value)
                        ), new Literal(2, new Profession(Kind.Float)))),
                            //new Variable(it, new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
                            //    { Imp.operation("+",
                            //        new Variable(it, new Property_Expression(constraint.endpoints.Last())),
                            //        new Variable(offset)) })),
                            //new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
                            //    { Imp.operation("+", new Variable(value), new Variable(offset)) }),

                            new Declare_Variable(conflict, new Instantiate(conflict_dungeon)),
                            //new Variable(conflict, new Function_Call("initialize")),
                            new Variable(conflict,Imp.setter(conflict_nodes, new Self(dungeon), null, null)),
                            new Variable(conflict,Imp.setter(conflict_nodes, new Variable(it), null, null)),
                            new Tie_Expression(mold_tie,
                                new Tie_Expression(piecemaker_tie,
                                    Imp.setter(conflicts_tie, new Variable(conflict), null, null)
                            )),
                            new Statement("return", Imp.False())

                    })
            };
        }

        static Dungeon create_conflict_class(Constraint constraint, Rail rail, Overlord overlord)
        {
            var previous = overlord.get_dungeon(rail);

            var context = new Summoner.Context(previous.realm);
            context.add_pattern("Node_Type", new Profession(Kind.reference, previous));
            context.add_pattern("Class_Name", "Distance_Conflict");
            context.add_pattern("Class_Name", "Distance_Conflict");

            var result = overlord.summon_dungeon(Piece_Maker.templates["Distance_Conflict"], context);
            var portal = result.all_portals["nodes"];
            var scope = new Scope();
            scope.add_map("a", () => new Portal_Expression(portal) { index = new Literal((int)0) });
            scope.add_map("b", () => new Portal_Expression(portal) { index = new Literal((int)1) });
            var imp = result.summon_imp("is_resolved");
            imp.expressions.Add(new Statement("return",
                new Operation(constraint.op, new List<Expression>{ 
                    new Portal_Expression(portal, new Tie_Expression(constraint.endpoints.Last(),
                            new Function_Call("dist", null,
                                new List<Expression> { new Portal_Expression(portal, new Tie_Expression(constraint.endpoints.Last()
                                    )) { index = new Literal((int)1) } }, true)))
                                { index = new Literal((int)0) },
                    overlord.translate(constraint.second.First(), scope)
                })
            ));
            return result;
            //var base_class = overlord.realms["piecemaker"[.dungeons["Conflict"];
            //var result = new Dungeon("Distance_Conflict", overlord, dungeon.realm, base_class);
            //var portal = result.add_portal(new Portal("nodes", Kind.list, result, dungeon));
            //result.generate_code1();

            //var scope = new Scope();


            //return result;
        }
    }
}