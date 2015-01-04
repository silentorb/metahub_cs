using System;
using metahub.logic.schema;
using metahub.logic.types;

namespace metahub.imperative.types
{

    public delegate Expression Expression_Generator();
/**
 * @author Christopher W. Johnson
 */
public class Expression {
    public Expression_Type type;
	public Expression child = null;
	
	protected Expression(Expression_Type type, Expression child = null) {
		this.type = type;
	    this.child = child;
	}

    public virtual Signature get_signature()
    {
        throw new Exception("Not implemented.");
    }
}
//struct Node {
	//Expression_Type type,
	//object value,
	//Node child,
	//string name,
	//List<object> args,
	//List<object> path,
	//Tie tie,
	//bool is_platform_specific
//}
}