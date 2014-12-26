package metahub.logic.schema ;
using metahub.imperative.Imp;
using metahub.logic.schema.Railway;
using metahub.meta.types.Expression;
using metahub.meta.types.Lambda;
//import metahub.logic.schema.Signature;
//import metahub.imperative.types.Expression;
using metahub.meta.types.Function_Call;
using metahub.meta.types.Path;
using metahub.meta.types.Property_Expression;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Constraint {
  public Expression reference;
  public Expression expression;
	public bool is_back_referencing = false;
	public string operator;
	public List<Constraint> other_constraints = new List<Constraint>();
	public Lambda lambda;
	//public List<Expression> block = null;

	public Constraint(metahub expression.meta.types.Constraint, Imp imp) {
		operator = expression.operator;
		reference = expression.first;
		this.expression = expression.second;
		this.lambda = expression.lambda;
	}

}