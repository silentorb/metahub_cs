using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;
using metahub.meta.types;
using Function_Call = metahub.imperative.types.Function_Call;
using Path = metahub.imperative.types.Path;

namespace metahub.imperative.code
{

    public class Parse
    {

        public static Tie get_start_tie(Node expression)
        {
            var path = (Reference_Path)expression;
            var property_expression = (Property_Reference)path.children[0];
            return property_expression.tie;
        }

        public static Tie get_end_tie(Node expression)
        {
            var path = get_path(expression);
            var i = path.Count;
            while (--i >= 0)
            {
                if (!path[i].rail.trellis.is_value)
                    return path[i];
            }

            throw new Exception("Could not find property inside Node path.");
        }

        //public static Tie get_end_tie (Node Node) {
        //Reference_Path path = Node;
        //var i = path.children.Count;
        //while (--i >= 0) {
        //if (path.children[i].type == Expression_Type.property) {
        //metahub.imperative.types.Property_Reference property_expression = path.children[i];
        //if (property_expression.tie.rail.trellis.is_value)
        //continue;
        //
        //return property_expression.tie;
        //}
        //}
        //
        //throw new Exception("Could not find property inside Node path.");
        //}

        public static List<Tie> get_path(Node expression)
        {
            switch (expression.type)
            {

                case Node_Type.path:
                    return get_path_from_array(((Reference_Path)expression).children);

                case Node_Type.array:
                    return get_path_from_array(((Array_Expression)expression).children);

                case Node_Type.property:
                    return new List<Tie> { ((Property_Reference)expression).tie };

                case Node_Type.function_call:
                    //Function_Call function_call = expression;
                    return new List<Tie>();

                case Node_Type.variable:
                    return new List<Tie>();

                default:
                    return new List<Tie>();
                //throw new Exception("Unsupported path Node type: " + Node.type);
            }
        }

        public static List<Tie> get_path_from_array(List<Node> expressions)
        {
            List<Tie> result = new List<Tie>();
            foreach (var token in expressions)
            {
                result = result.Union(get_path(token)).ToList();
            }

            return result;
        }

        public static List<Node> normalize_path(Node expression)
        {
            switch (expression.type)
            {

                case Node_Type.path:
                    var path = expression as Reference_Path;
                    var result = new List<Node>();
                    return path.children.Aggregate(result, (current, token) =>
                        current.Union(normalize_path(token)).ToList()
                    );

                case Node_Type.function_call:
                    //Function_Call function_call = expression;
                    //if (function_call.input != null)
                    //return normalize_path(function_call.input);

                    return new List<Node> { expression };

                default:
                    return new List<Node> { expression };
            }
        }


        public static Node resolve(Node expression)
        {
            switch (expression.type)
            {

                case Node_Type.path:
                    var path = (Reference_Path)expression;
                    return path.children[path.children.Count - 1];

                case Node_Type.function_call:
                    throw new Exception("Not implemented.");

                default:
                    return expression;
            }
        }

        public static List<Tie> reverse_path(IEnumerable<Tie> path)
        {
            return path.Select((t) => t.other_tie).Reverse().ToList();
        }

    }
}