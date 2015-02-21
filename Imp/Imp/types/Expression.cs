using System;
using System.Collections.Generic;
using metahub.imperative.schema;
using metahub.imperative.summoner;



namespace metahub.imperative.types
{

    public delegate Expression Expression_Generator(Summoner.Context context);
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

    public string stack_trace;

    public Expression parent = null;
	
	protected Expression(Expression_Type type, Expression child = null) {
        stack_trace = Environment.StackTrace;
		this.type = type;
	    this.child = child;
	    if (child != null)
	        child.parent = this;
	}

    public virtual Profession get_profession()
    {
        throw new Exception("Not implemented.");
    }

    public Expression get_end()
    {
        var result = this;
        while (result.child != null && (result.child.type == Expression_Type.property || result.child.type == Expression_Type.portal))
        {
            result = result.child;
        }

        return result;
    }

    public List<Expression> get_chain()
    {
        var result = new List<Expression>();
        var current = this;
        while (current != null && (current.type == Expression_Type.property || current.type == Expression_Type.portal))
        {
            result.Add(current);
            current = current.child;
        }

        return result;
    }

    public virtual Expression clone()
    {
        throw new Exception("Not implemented.");
    }

    public void disconnect_parent()
    {
        if (parent == null)
            throw new Exception("Cannot disconnect parent.");

        if (parent.child != this)
            throw new Exception("parent child mixup.");

        parent.child = null;
        parent = null;
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