using System.Collections.Generic;
using System.Linq;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.meta.types;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;

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

	public static List<Expression> constraint (Constraint constraint, Imp imp) {
		var op = constraint.op;
        //return new List<Expression>();
		var reference = imp.translate(constraint.reference);

		if (op == "in") {
			var args = (metahub.meta.types.Block)constraint.expression;
            return generate_constraint(reference, ">=", (Literal_Value)args.children[0])
			.Union(
                generate_constraint(reference, "<=", (Literal_Value)args.children[1])
			).ToList();
		}

		return generate_constraint(reference, constraint.op, (Literal_Value)constraint.expression);

	}

    static List<Expression> generate_constraint(Expression reference, string op, Literal_Value literal)
    {
		var inverse = inverse_operators[op];
		float limit = (float)literal.value;

		const float min = 0.0001f;
		float value = 0;
		switch(op) {
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

		return new List<Expression> { new Flow_Control("if",	new Condition(inverse,
				new List<Expression> {
					reference,
				new Literal(limit, new Signature(Kind.Float))
        }
			),
		new List<Expression>{
				new Assignment(reference, "=", new Literal(value, new Signature { type = Kind.Float }))
    }
		)};
	}

	//public static Node convert_expression (metahub Node.meta.types.Node, Scope scope) {
//
		//
//
	//}

}
}