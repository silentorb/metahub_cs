package metahub.imperative.code ;
using metahub.imperative.Imp;
using metahub.logic.schema.Constraint;
using metahub.logic.schema.Rail;
using metahub.logic.schema.Railway;
using metahub.imperative.types.*;
using metahub.meta.Scope;
using metahub.schema.Kind;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Reference
{
	public static var inverse_operators = {
		">": "<=",
		"<": ">=",
		">=": "<",
		"<=": ">"
	}

	public static List<Expression> constraint (Constraint constraint, Imp imp) {
		var operator = constraint.operator;
		return [];
		var reference = imp.translate(constraint.reference);

		if (operator == "in") {
			metahub.meta.types.Block args = cast constraint.expression;
			return generate_constraint(reference, ">=", cast args.children[0])
			.concat(
				generate_constraint(reference, "<=", cast args.children[1])
			);
		}

		return generate_constraint(reference, constraint.operator, cast constraint.expression);
		//var inverse = inverse_operators[operator];
		//Literal conversion = cast constraint.expression;
		//float limit = conversion.value;
//
		//float min = 0.0001;
		//float value = 0;
		//switch(operator) {
			//case "<":
				//value = limit - min;
			//case ">":
				//value = limit + min;
			//case "<=":
				//value = limit;
			//case ">=":
				//value = limit;
		//}
//
		//return [ new Flow_Control("if",	new Condition(inverse,
				//[
					//imp.translate(constraint.reference),
					//{ type: Expression_Type.literal, value: limit }
				//]
			//),
			//[
				//new Assignment(imp.translate(constraint.reference), "=", new Literal(value, { type: Kind.Float }))
			//]
		//)];
	}

	static List<Expression> generate_constraint (Expression reference, operator, Literal literal) {
		var inverse = inverse_operators[operator];
		float limit = literal.value;

		float min = 0.0001;
		float value = 0;
		switch(operator) {
			case "<":
				value = limit - min;
			case ">":
				value = limit + min;
			case "<=":
				value = limit;
			case ">=":
				value = limit;
		}

		return [ new Flow_Control("if",	new Condition(inverse,
				[
					reference,
					{ type: Expression_Type.literal, value: limit }
				]
			),
			[
				new Assignment(reference, "=", new Literal(value, { type: Kind.Float }))
			]
		)];
	}

	//public static Expression convert_expression (metahub expression.meta.types.Expression, Scope scope) {
//
		//
//
	//}

}