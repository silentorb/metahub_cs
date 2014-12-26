namespace metahub.meta.types
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
public class Function_Scope : Expression
    {
        public Expression expression;
        public Lambda lambda;

        public Function_Scope(Expression expression, Lambda lambda)
            : base(Expression_Type.function_scope)
        {
            this.expression = expression;
            this.lambda = lambda;
        }

    }
}