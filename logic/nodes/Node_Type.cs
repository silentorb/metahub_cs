namespace metahub.logic.nodes
{
    public enum Node_Type
    {
        // Expressions
        constraint,
        literal,
        property,
        variable,
        function_call,
        instantiate,
        parent_class,
        path,
        lambda,
        Null,
        array,
        parameter,

        // Statements
        space,
        class_definition,
        function_definition,
        flow_control,
        assignment,
        declare_variable,
        scope,
        block,
        constra,
        function_scope,

        // Internal
        scope_node,
        bounce
    }
}