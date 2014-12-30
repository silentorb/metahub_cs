using System.Collections.Generic;
using System.Linq;
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

        public static List<Expression> constraint(Constraint constraint, Tie tie, Imp imp)
        {
            var op = constraint.op;
            //return new List<Expression>();
            var first_last = constraint.first.Last();
            if (first_last.type == Node_Type.function_call
                && ((metahub.logic.types.Function_Call)first_last).name == "dist")
            {
                return dist(constraint, tie);
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

            return new List<Expression> { new Flow_Control("if", new Condition(inverse,
				new List<Expression> {
					reference,
				new Literal(limit, new Signature(Kind.Float))
            }),
		        new List<Expression>{
				    new Assignment(reference, "=", new Literal(value, new Signature { type = Kind.Float }))
                }
		    )};
        }

        public List<Expression> dist(Constraint constraint, Tie tie, Imp imp)
        {
            //var dungeon = imp.get_dungeon(tie.rail);
            //dungeon.concat_block(tie.tie_name + "_set_pre", Reference.constraint(constraint, this));

            //return new List<Expression>();
        }

    }
}