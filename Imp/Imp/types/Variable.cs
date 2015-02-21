using System;
using metahub.imperative.schema;


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

        public override Profession get_profession()
        {
            return symbol.profession;
        }

        public override Expression clone()
        {
            return new Variable(symbol, child != null ? child.clone() : null)
            {
                index = index != null ? index.clone() : null
            };
        }
    }
}