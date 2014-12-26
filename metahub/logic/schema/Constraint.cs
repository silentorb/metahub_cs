using System.Collections.Generic;
using metahub.imperative;
using metahub.meta.types;

//import metahub.logic.schema.Signature;
//import metahub.imperative.types.Expression;

/**
 * ...
 * @author Christopher W. Johnson
 */
namespace metahub.logic.schema {
public class Constraint {
  public Expression reference;
  public Expression expression;
	public bool is_back_referencing = false;
	public string op;
	public List<Constraint> other_constraints = new List<Constraint>();
	public Lambda lambda;
	//public List<Expression> block = null;

	public Constraint(metahub.meta.types.Constraint expression, Imp imp) {
		op = expression.op;
		reference = expression.first;
		this.expression = expression.second;
		this.lambda = expression.lambda;
	}

}
}