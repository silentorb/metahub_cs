using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;
using metahub.logic.nodes;
using metahub.schema;

namespace imperative.code
{

    public class Parse
    {

        public static Property get_start_tie(Node expression)
        {
            var path = (Reference_Path)expression;
            var property_expression = (Property_Node)path.children[0];
            return property_expression.property;
        }

        public static Property get_end_tie(Node expression)
        {
            var path = get_path(expression);
            var i = path.Count;
            while (--i >= 0)
            {
                if (!path[i].trellis.is_value)
                    return path[i];
            }

            throw new Exception("Could not find property inside Node path.");
        }

        public static Property get_end_tie(Node[] path)
        {
            var i = path.Length;
            while (--i >= 0)
            {
                var token = path[i] as Property_Node;
                if (token == null)
                    continue;

                if (!token.property.trellis.is_value)
                    return token.property;
            }

            //throw new Exception("Could not find property inside Node path.");
            return null;
        }

        public static List<Property> get_endpoints(Node[] path)
        {
            var i = path.Length;
            while (--i >= 0)
            {
                var token = path[i];
                switch (token.type)
                {
                    case Node_Type.property:
                        var prop = (Property_Node)token;
                        if (!prop.property.trellis.is_value)
                            return new List<Property> { prop.property };

                        break;

                    case Node_Type.array:
                        var result = new List<Property>();
                        foreach (var t in ((Array_Expression)token).children)
                        {
                            result.AddRange(get_endpoints(new Node[] { t }).Distinct());
                        }
                        return result;

                    case Node_Type.path:
                        return get_endpoints(((Reference_Path)token).children.ToArray());
                        break;
                }

            }

            return new List<Property>();
            //throw new Exception("Could not find endpo inside Node path.");
        }


        //public static Property get_end_tie (Node Node) {
        //Reference_Path path = Node;
        //var i = path.children.Count;
        //while (--i >= 0) {
        //if (path.children[i].type == Expression_Type.property) {
        //imperative.types.Property_Reference property_expression = path.children[i];
        //if (property_expression.tie.rail.trellis.is_value)
        //continue;
        //
        //return property_expression.tie;
        //}
        //}
        //
        //throw new Exception("Could not find property inside Node path.");
        //}

        public static List<Property> get_path(Node expression)
        {
            return expression.get_path().OfType<Property_Node>().Select(n => n.property).ToList();

            switch (expression.type)
            {

                case Node_Type.path:
                    return get_path_from_array(((Reference_Path)expression).children);

                case Node_Type.array:
                    return get_path_from_array(((Array_Expression)expression).children);

                case Node_Type.property:
                    return new List<Property> { ((Property_Node)expression).property };

                case Node_Type.function_call:
                    //Function_Call function_call = expression;
                    return new List<Property>();

                case Node_Type.variable:
                    return new List<Property>();

                default:
                    return new List<Property>();
                //throw new Exception("Unsupported path Node type: " + Node.type);
            }
        }

        public static List<Property> get_path_from_array(IEnumerable<Node> expressions)
        {
            List<Property> result = new List<Property>();
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

        public static List<Property> reverse_path(IEnumerable<Property> path)
        {
            return path.Select((t) => t.other_property).Reverse().ToList();
        }

    }
}