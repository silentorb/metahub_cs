using System.Collections.Generic;
using metahub.imperative;
using metahub.meta.types;

//import metahub.logic.schema.Signature;
//import metahub.imperative.types.Node;

/**
 * ...
 * @author Christopher W. Johnson
 */
namespace metahub.logic.schema {
public class Constraint {
  public Node reference;
  public Node expression;
	public bool is_back_referencing = false;
	public string op;
	public List<Constraint> other_constraints = new List<Constraint>();
	public Lambda lambda;
	//public List<Node> block = null;

	public Constraint(metahub.meta.types.Constraint expression, Imp imp) {
		op = expression.op;
		reference = expression.first;
		this.expression = expression.second;
		this.lambda = expression.lambda;
	}

}
}