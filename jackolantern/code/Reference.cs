using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.logic;
using metahub.logic.schema;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Node_Type = metahub.logic.nodes.Node_Type;

namespace metahub.jackolantern.code
{
    public class Reference
    {
        public static void generate_constraint(Constraint constraint, Tie tie, JackOLantern jack)
        {
            //if (constraint.op == "!="
            //            //&& constraint.second.Length == 1 
            //            && constraint.second.type == Node_Type.Null)
            //{
            //    not_null((Portal_Expression)jack.convert_path(constraint.first.get_path(), null), jack);
            //}
            //else if (Constraint.circular_operators.Contains(constraint.op))
            //{
            //    self_modifying(constraint, tie, jack);
            //}
            //else
            //{
            //    var dungeon = jack.overlord.get_dungeon(tie.rail);
            //    var block = dungeon.get_block("set_" + tie.tie_name);
            //    var constraint_code = Reference.constraint(constraint, tie, jack, block.scope);
            //    if (constraint_code != null)
            //        block.add_many("pre", constraint_code);
            //}
        }

        public static void self_modifying(Constraint constraint, Tie tie, JackOLantern jack)
        {
            /*
            var property_node = (metahub.logic.nodes.Property_Reference)constraint.first;
            var dungeon = overlord.get_dungeon(property_node.tie.rail);
            var minion = dungeon.summon_imp("tick");
            var first_path = constraint.first.get_path();
            var path = first_path.Take(first_path.Count - 1).ToArray();
            var path1 = overlord.convert_path(path, null);
            var path2 = overlord.convert_path(first_path, null);
            var signature = path1.get_signature();
            var other_dungeon = overlord.get_dungeon(signature.rail);
            //var tie = ((metahub.logic.types.Property_Reference)first).tie;
            var portal = other_dungeon.all_portals[tie.tie_name];
            path1.child = new Property_Function_Call(Property_Function_Type.set,
                portal, new List<Expression>
                    {
                      new Operation(constraint.op.Substring(0, 1), new List<Expression>
                          {
                              path2,
                              overlord.convert_path(constraint.second.get_path(), null)
                          })  
                    }
                );
            minion.expressions.Add(path1);
            //minion.expressions.Add(new Assignment(
            //    overlord.convert_path(constraint.first, null),
            //    constraint.op,
            //    overlord.convert_path(constraint.second, null)
            //    ));
             * */
        }

        public static List<Expression> constraint(Constraint constraint, Tie tie, JackOLantern jack, Scope scope)
        {
            throw new Exception();
            //var op = constraint.op;
            ////return new List<Expression>();
            ////var first_last = constraint.first.Last();
            ////if (first_last.type == Node_Type.function_call
            ////    && ((metahub.logic.types.Function_Call)first_last).name == "distance")
            //if (constraint.constraint_scope != null)
            //{
            //    switch (constraint.constraint_scope.name)
            //    {
            //        case "cross":
            //            return cross(constraint, tie, jack, scope);
            //        case "map":
            //            //List_Code.map(constraint, constraint.second, overlord);
            //            return new List<Expression>();
            //    }

            //}

            //var reference = jack.translate(constraint.first, scope);

            ////if (constraint.first.)

            //if (op == ">=<")
            //{
            //    var args = ((metahub.logic.nodes.Array_Expression)constraint.second).children;
            //    return generate_constraint(reference, ">=", jack.translate(args[0]))
            //        .Union(
            //            generate_constraint(reference, "<=", jack.translate(args[1]))
            //        ).ToList();
            //}

            //return generate_constraint(reference, constraint.op, jack.translate(constraint.second));
        }

        public static void not_null(Portal_Expression reference, JackOLantern jack)
        {
            var portal = reference.portal;
            var dungeon = portal.dungeon;
            var other_dungeon = portal.other_dungeon;
            var block = dungeon.get_block("initialize");

            block.add_many("pre", new List<Expression>
                {
                  new Assignment(new Portal_Expression(portal), "=", new Instantiate(other_dungeon)),
                  Minion.call_initialize(dungeon, other_dungeon, new Portal_Expression(portal))
                });
        }

        static List<Expression> generate_constraint(Expression reference, string op, Expression literal)
        {
            Expression value = literal;
            if (op != "=")
            {
                //float limit = literal.get_float();

                const float min = 0.0001f;
                switch (op)
                {
                    case "<":
                        value = new Operation("-", new List<Expression> {literal, new Literal(min)});
                        break;

                    case ">":
                        value = new Operation("+", new List<Expression> {literal, new Literal(min)});
                        break;

                    //case "<=":
                    //    value = literal;
                    //    break;

                    //case ">=":
                    //    value = literal;
                    //    break;
                }

                op = Logician.inverse_operators[op];
            }

            return new List<Expression> { new Flow_Control(Flow_Control_Type.If, new Operation(op,
				new List<Expression> {
					reference, value
            }),
		        new List<Expression>{
				    new Assignment(reference, "=", value)
                }
		    )};
        }

        public static List<Expression> cross(Constraint constraint, Tie tie, JackOLantern jack, Scope scope)
        {
            //var dungeon = minion.get_dungeon(tie.rail);
            //dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, this));
            var property_reference = (metahub.logic.nodes.Property_Node)constraint.constraint_scope.caller.First();
            var dungeon = jack.get_dungeon(tie.rail);

            var iterator_scope = new Scope(scope);
            var it = iterator_scope.create_symbol("item", new Profession(Kind.reference, jack.get_dungeon(property_reference.tie.other_rail)));
            iterator_scope.add_map("a", c => new Variable(it));
            iterator_scope.add_map("b", c => new Self(dungeon));

            var class_block = dungeon.get_block("class_definition");
            var function_scope = new Scope(class_block.scope);
            var value = function_scope.create_symbol("value", jack.get_profession(tie.get_signature()));
            var function_name = "check_" + tie.tie_name + "_" + property_reference.tie.other_tie.tie_name;
            var portal = jack.get_portal(property_reference.tie);
            var minion = dungeon.spawn_minion(function_name, new List<Parameter>
                {
                    new Parameter(value)
                }, new List<Expression>
                    {
                        Minion.If(new Operation("==", new List<Expression>
                            {
                                new Portal_Expression(portal.other_portal),
                                new Null_Value()
                            }), new List<Expression>
                                {
                                    new Statement("return", Minion.True())
                                }),
                        new Iterator(it,
                                     new Portal_Expression(portal.other_portal,
                                                        new Portal_Expression(portal)), 
                                     new List<Expression>
                                         {
                                             Minion.If(new Operation("==", new List<Expression>
                                                 {
                                                     new Variable(it),
                                                     new Self(dungeon)
                                                 }), new List<Expression>
                                                     {
                                                         new Statement("continue")
                                                     })

                                         }.Concat(distance(constraint, tie, jack, iterator_scope, it, value))
                                          .ToList()),
                        new Statement("return", new Literal(true))
                    }, new Profession(Kind.Bool));

            var setter_block = dungeon.get_block("set_" + property_reference.tie.other_tie.tie_name);
            setter_block.add("post", new Class_Function_Call(function_name, null, new Expression[]
                {
                    new Portal_Expression(jack.get_portal(constraint.endpoints.First()))
                })
            );

            //return new List<Expression>();
            return new List<Expression>
                {
                    new Flow_Control(Flow_Control_Type.If, 
                    new Operation("==", new List<Expression>{ 
                    new Class_Function_Call(function_name, null, new Expression[]
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

        public static List<Expression> distance(Constraint constraint, Tie tie, JackOLantern jack, Scope scope, Symbol it, Symbol value)
        {
            throw new Exception();
            //var offset = scope.create_symbol("offset", value.signature);
            //var dungeon = jack.overlord.get_dungeon(tie.rail);
            //var conflict_dungeon = create_conflict_class(constraint, tie.rail, jack);
            //var conflict_nodes = conflict_dungeon.all_portals["nodes"];
            //var conflict = scope.create_symbol("conflict", new Profession(Kind.reference, conflict_dungeon));
            //var mold_tie = tie.rail.get_tie_or_error("mold");
            //var piecemaker_tie = mold_tie.other_rail.get_tie_or_error("piece_maker");
            //var conflicts_tie = piecemaker_tie.other_rail.get_tie_or_error("conflicts");

            //return new List<Expression>
            //{
            //    Minion.If(Minion.operation(Logician.inverse_operators[constraint.op], 
            //        new Platform_Function("distance", new Variable(it, new Portal_Expression(jack.overlord.get_portal(constraint.endpoints.Last()))),
            //            new List<Expression> { new Variable(value) }),
            //            jack.translate(constraint.second, scope)
            //        ),
            //        new List<Expression>
            //        {
            //            new Declare_Variable(offset, Minion.operation("/", Minion.operation("+",
            //                new Variable(it, new Portal_Expression(jack.overlord.get_portal(constraint.endpoints.Last()))),
            //                new Variable(value)
            //            ), new Literal(2, new Profession(Kind.Float)))),
            //                //new Variable(it, new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
            //                //    { Minion.operation("+",
            //                //        new Variable(it, new Property_Expression(constraint.endpoints.Last())),
            //                //        new Variable(offset)) })),
            //                //new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
            //                //    { Minion.operation("+", new Variable(value), new Variable(offset)) }),

            //                new Declare_Variable(conflict, new Instantiate(conflict_dungeon)),
            //                new Variable(conflict,Minion.setter(conflict_nodes, new Self(dungeon), null, null)),
            //                new Variable(conflict,Minion.setter(conflict_nodes, new Variable(it), null, null)),
            //                new Portal_Expression(jack.overlord.get_portal(mold_tie),
            //                    new Portal_Expression(jack.overlord.get_portal(piecemaker_tie),
            //                        Minion.setter(conflicts_tie, new Variable(conflict), null, null)
            //                )),
            //                new Statement("return", Minion.False())

            //        })
            //};
        }

        static Dungeon create_conflict_class(Constraint constraint, Rail rail, JackOLantern jack)
        {
            throw new Exception();
            //var previous = jack.overlord.get_dungeon(rail);

            //var context = new Summoner.Context(previous.realm);
            //context.set_pattern("Node_Type", new Profession(Kind.reference, previous));
            //context.set_pattern("Class_Name", "Distance_Conflict");
            //context.set_pattern("Class_Name", "Distance_Conflict");

            //var result = jack.overlord.summon_dungeon(Piece_Maker.templates["Distance_Conflict"], context);
            //var portal = result.all_portals["nodes"];
            //var scope = new Scope();
            //scope.add_map("a", c => new Portal_Expression(portal) { index = new Literal((int)0) });
            //scope.add_map("b", c => new Portal_Expression(portal) { index = new Literal((int)1) });
            //var minion = result.summon_imp("is_resolved");
            //minion.expressions.Add(new Statement("return",
            //    new Operation(constraint.op, new List<Expression>{ 
            //        new Platform_Function("distance", new Portal_Expression(portal, 
            //            new Portal_Expression(jack.overlord.get_portal(constraint.endpoints.Last())))
            //            { index = new Literal((int)0) },
            //            new List<Expression> { new Portal_Expression(portal, new Portal_Expression(jack.overlord.get_portal(constraint.endpoints.Last())
            //                )) { index = new Literal((int)1) } }),
            //        jack.translate(constraint.second, scope)
            //    })
            //));
            //return result;


            //var base_class = overlord.realms["piecemaker"[.dungeons["Conflict"];
            //var result = new Dungeon("Distance_Conflict", overlord, dungeon.realm, base_class);
            //var portal = result.add_portal(new Portal("nodes", Kind.list, result, dungeon));
            //result.generate_code1();

            //var scope = new Scope();


            //return result;
        }
    }
}