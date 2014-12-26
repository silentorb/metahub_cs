namespace metahub.imperative.types
{

    /**
     * @author Christopher W. Johnson
     */

    public enum Expression_Type
    {
        // Expressions
        literal = 1,
        property = 2,
        variable = 3,
        function_call = 4,
        instantiate = 5,
        parent_class = 6,

        create_array = 8,
        null_value = 9,
        self = 10,

        path = 200,

        // Statements
        statement = 99,
        space = 100,
        class_definition = 101,
        function_definition = 102,
        flow_control = 103,
        assignment = 104,
        declare_variable = 105,
        scope = 106,
        insert = 107
    }
}