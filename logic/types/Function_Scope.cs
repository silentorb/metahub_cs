namespace metahub.logic.types
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
public class Function_Scope : Node
    {
        public Node[] expression;
        public Lambda lambda;

        public Function_Scope(Node[] expression, Lambda lambda)
            : base(Node_Type.function_scope)
        {
            this.expression = expression;
            this.lambda = lambda;
        }

    }
}