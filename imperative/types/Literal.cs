using System;
using metahub.imperative.schema;
using metahub.logic.schema;
using Kind = metahub.schema.Kind;

namespace metahub.imperative.types {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Literal : Expression {
	public object value;
    //public Signature signature;
    public Profession profession;

    //public Literal(object value, Signature signature)

    //    : base(Expression_Type.literal)
    //{
    //    this.value = value;
    //    this.signature = signature;
    //}

    public Literal(object value, Profession profession)

        : base(Expression_Type.literal)
    {
        //if (value == null)
        //   throw new Exception("Literal value cannot be null.");

        this.value = value;
        this.profession = profession;
    }

    public Literal(int value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        profession = new Profession(Kind.Int);
    }

    public Literal(string value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        profession = new Profession(Kind.String);
    }

    public Literal(float value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        profession = new Profession(Kind.Float);
    }

    public Literal(bool value)

        : base(Expression_Type.literal)
    {
        this.value = value;
        profession = new Profession(Kind.Bool);
    }

    public override Expression clone()
    {
        return new Literal(value, profession);
    }

    public float get_float()
    {
        return profession.type == Kind.Int 
            ? (int) value 
            : (float) value;
    }
}}