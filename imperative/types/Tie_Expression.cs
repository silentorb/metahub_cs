using metahub.logic.schema;
using metahub.logic.types;
using metahub.schema;

namespace metahub.imperative.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Tie_Expression : Expression
{
	public Tie tie;
    public Expression index;
	
	public Tie_Expression(Tie tie, Expression child = null)

        : base(Expression_Type.property, child)
    {
		this.tie = tie;
	}

    public override Signature get_signature()
    {
        //if (child != null && child.type == Expression_Type.property)
        //    return child.get_signature();

        return index == null
            ? tie.get_signature()
            : new Signature(Kind.reference, tie.other_rail);
    }

    public override Expression clone()
    {
        return new Tie_Expression(tie, child != null ? child.clone(): null)
            {
                index = index != null ? index.clone() : null
            };
    }
	
}}