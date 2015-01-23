using System;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.nodes;

namespace metahub.imperative.types
{

    public delegate Expression Expression_Generator();
/**
 * @author Christopher W. Johnson
 */
public class Expression {
    public Expression_Type type;
    private Expression _child;
    public Expression child
    {
        get { return _child; }
        set { _child = value;
            if (_child != null)
                _child.parent = this;
        }
    }

    public Expression parent = null;
	
	protected Expression(Expression_Type type, Expression child = null) {
		this.type = type;
	    this.child = child;
	    if (child != null)
	        child.parent = this;
	}

    public virtual Signature get_signature()
    {
        throw new Exception("Not implemented.");
    }

    public virtual Profession get_profession()
    {
        throw new Exception("Not implemented.");
    }

    public static Expression get_end(Expression expression)
    {
        var result = expression;
        while (result.child != null && (result.child.type == Expression_Type.property || result.child.type == Expression_Type.portal))
        {
            result = result.child;
        }

        return result;
    }

    public virtual Expression clone()
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