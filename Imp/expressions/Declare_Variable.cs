using metahub.imperative.schema;



namespace metahub.imperative.expressions
{
    public class Declare_Variable : Expression
    {
        public Symbol symbol;
        public Expression expression;

        public Declare_Variable(Symbol symbol, Expression expression)
            : base(Expression_Type.declare_variable)
        {
            this.symbol = symbol;
            this.expression = expression;
        }

        public override Expression clone()
        {
            return new Declare_Variable(symbol, expression);
        }
    }

}