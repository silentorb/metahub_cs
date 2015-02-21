namespace metahub.imperative.types
{
    public enum Expression_Type
    {
        // Expressions
        literal,
        property,
        variable,
        function_call,
        property_function_call,
        platform_function,
        instantiate,
        parent_class,
        operation,
        create_array,
        null_value,
        self,
        path,
        portal,
        comment,

        // Statements
        statement,
        space,
        class_definition,
        function_definition,
        flow_control,
        assignment,
        declare_variable,
        scope,
        insert,
        iterator,

        // Summoner
        statements,
        snippet
    }
}