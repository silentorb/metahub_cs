using System;
using System.Collections.Generic;
using System.Linq;
using metahub.parser.types;

namespace metahub.parser
{
    //import metahub.code.functions.Functions;

    class Assignment_Source
    {
        public string type;
        public List<string> path;
        public object expression;
        public string modifier;
    }

    class Reference_Or_Function
    {
        public string type;
        //List<string> path,
        public string name;
        public object expression;
        public List<object> inputs;
    }

    public class MetaHub_Context : Parser_Context
    {

        //private static Map<string, Functions> function_Dictionary;

        public MetaHub_Context(Definition definition)
            : base(definition)
        {

        }

        public override object perform_action(string name, Pattern_Source data, Match match)
        {

            switch (match.pattern.name)
            {
                case "reference":
                    data = reference(data, (Repetition_Match)match);
                    break;

                case "start":
                    data = data.patterns[1];
                    break;

                case "array":
                case "long_block":
                    data = data.patterns[2];
                    break;

                case "parameters":
                    data = data.patterns[1];
                    break;

                case "arguments":
                    data = data.patterns[3];
                    break;

                case "expression":
                    expression(data, (Repetition_Match)match);
                    break;

                case "complex_token":
                    if (data.patterns[1].patterns.Length == 0)
                    {
                        return data.patterns[0];
                    }
                    break;
            }

            if (data.type == null)
                data.type = match.pattern.name;

            //Console.WriteLine(data.type ?? "null", match.pattern.name);

            return data;
            //switch (match.pattern.name)
            //{
            //    case "start":
            //        return start(data);

            //    case "create_symbol":
            //        return create_symbol(data);

            //    //case "create_node":
            //    //    return create_node(data);

            //    case "create_constraint":
            //        return create_constraint(data);

            //    case "Node":
            //        return expression(data, match);

            //    //case "method":
            //    //    return method(data);

            //    case "reference":
            //        return reference(data, (Repetition_Match) match);

            //    case "long_block":
            //        return long_block(data);

            //    case "set_property":
            //        return set_property(data);

            //    case "new_scope":
            //        return new_scope(data);

            //    case "constraint_block":
            //        return constraint_block(data);

            //    case "constraint":
            //        return constraint(data);

            //    case "condition":
            //        return condition(data);

            //    case "conditions":
            //        return conditions(data, match);

            //    case "condition_block":
            //        return condition_block(data);

            //    case "if":
            //        return if_statement(data);

            //    case "string":
            //        return data.patterns[1].text;

            //    case "bool":
            //        return data.text == "true";

            //    case "int":
            //        return int.Parse(data.text);

            //    case "value":
            //        return value(data);

            //    case "optional_block":
            //        return optional_block(data);

            //    case "set_weight":
            //        return set_weight(data);

            //    case "array":
            //        return array_expression(data);

            //    case "lambda":
            //        return lambda_expression(data);

            //    case "parameters":
            //        return parameters(data);

            //    case "function_scope":
            //        return function_scope(data);

            //    //      default:
            //    //        throw new Exception("Invalid parser method: " + name + ".");
            //}

            //return data;
        }

        //static Parser_Block start(Pattern_Source data)
        //{
        //    return new Parser_Block
        //    {
        //        type = "block",
        //        expressions = data.patterns[1]
        //    };
        //}

        //static Parser_Symbol create_symbol(Pattern_Source data)
        //{
        //    return new Parser_Symbol
        //    {
        //        type = "symbol",
        //        name = data.patterns[2],
        //        expression = (Parser_Item)data.patterns[6]
        //    };
        //}

        static Pattern_Source reference(Pattern_Source data, Repetition_Match match)
        {
            if (data.patterns.Length < 2)
                //return data;
                return data.patterns[0];

            var rep_match = match;
            //string op = rep_match.dividers[0].matches[1].get_data().text;
            var dividers = rep_match.dividers
                .Select(d => d.matches.First(m => m.pattern.name != "trim").get_data().text).ToList();


            for (var i = 1; i < data.patterns.Length; ++i)
            {
                var divider = dividers[i - 1];
                if (divider == "|")
                {
                    data.patterns[i].type = "function";
                }
            }

            return data;
        }

        public static Pattern_Source expression(Pattern_Source data, Repetition_Match match)
        {
            if (data.patterns.Length < 2)
                return data;

            var rep_match = match;
            var dividers = rep_match.dividers
                .Select(d => d.matches.First(m => m.pattern.name != "trim").get_data().text).ToList();

            var patterns = data.patterns.ToList();
            group_operators(new[] { "/", "*" }, patterns, dividers);
            group_operators(new[] { "+", "-" }, patterns, dividers);
            group_operators(new[] { ".." }, patterns, dividers);
            data.patterns = patterns.ToArray();

            return data;
        }

        public static void group_operators(string[] operators, List<Pattern_Source> patterns, List<string> dividers)
        {
            var i = 0;
            for (var d = 0; d < dividers.Count; ++d)
            {
                var divider = dividers[d];
                if (operators.Contains(divider))
                {
                    patterns[i] = new Pattern_Source()
                        {
                            type = "expression",
                            text = divider,
                            patterns = new[]
                                {
                                    patterns[i],
                                    patterns[i + 1]
                                }
                        };
                    patterns.RemoveAt(i + 1);
                    dividers.RemoveAt(d);
                    --d;
                }
                else
                {
                    ++i;
                }
            }
        }
    }
}