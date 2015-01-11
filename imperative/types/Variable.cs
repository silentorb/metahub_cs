using System;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.types;
using metahub.schema;

namespace metahub.imperative.types
{
    public class Variable : Expression
    {
        public Symbol symbol;
        public Expression index;

        public Variable(Symbol symbol, Expression child = null)
            : base(Expression_Type.variable, child)
        {
            if (symbol == null)
               throw new Exception("Variable symbol cannot be null.");

            this.symbol = symbol;
        }

        public override Signature get_signature()
        {
            return symbol.signature;
        }
    }
}