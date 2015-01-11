using metahub.logic.schema;
using Kind = metahub.schema.Kind;

namespace metahub.imperative.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Literal : Expression {
	public object value;
	public Signature signature;

	public Literal(object value, Signature signature)

        : base(Expression_Type.literal)
    {
		this.value = value;
		this.signature = signature;
	}

    public Literal(int value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        signature = new Signature(Kind.Int);
    }

    public Literal(string value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        signature = new Signature(Kind.String);
    }

    public Literal(float value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        signature = new Signature(Kind.Float);
    }

    public Literal(bool value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        signature = new Signature(Kind.Bool);
    }

    public override Expression clone()
    {
        return new Literal(value, signature);
    }
}}