namespace metahub.meta.types
{
    public enum Node_Type
    {
        // Expressions
        literal = 1,
        property = 2,
        variable = 3,
        function_call = 4,
        instantiate = 5,
        parent_class = 6,
        path = 7,
        lambda = 8,

        array = 12,

        // Statements
        space = 100,
        class_definition = 101,
        function_definition = 102,
        flow_control = 103,
        assignment = 104,
        declare_variable = 105,
        scope = 106,
        block = 107,
        constra = 108,
        function_scope = 109
    }
}