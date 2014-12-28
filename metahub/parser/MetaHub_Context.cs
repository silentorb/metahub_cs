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
    public class MetaHub_Context : Context
    {

        //private static Map<string, Functions> function_Dictionary;

        public MetaHub_Context(Definition definition)
            : base(definition)
        {

        }

        public override object perform_action(string name, Pattern_Source data, Match match)
        {
            //var name = match.pattern.name;
            switch (match.pattern.name)
            {
                case "start":
                    return start(data);

                case "create_symbol":
                    return create_symbol(data);

                //case "create_node":
                //    return create_node(data);

                case "create_constraint":
                    return create_constraint(data);

                case "Node":
                    return expression(data, match);

                //case "method":
                //    return method(data);

                case "reference":
                    return reference(data, (Repetition_Match) match);

                case "long_block":
                    return long_block(data);

                case "set_property":
                    return set_property(data);

                case "new_scope":
                    return new_scope(data);

                case "constraint_block":
                    return constraint_block(data);

                case "constraint":
                    return constraint(data);

                case "condition":
                    return condition(data);

                case "conditions":
                    return conditions(data, match);

                case "condition_block":
                    return condition_block(data);

                case "if":
                    return if_statement(data);

                case "string":
                    return data.patterns[1].text;

                case "bool":
                    return data.text == "true";

                case "int":
                    return int.Parse(data.text);

                case "value":
                    return value(data);

                case "optional_block":
                    return optional_block(data);

                case "set_weight":
                    return set_weight(data);

                case "array":
                    return array_expression(data);

                case "lambda":
                    return lambda_expression(data);

                case "parameters":
                    return parameters(data);

                case "function_scope":
                    return function_scope(data);

                //      default:
                //        throw new Exception("Invalid parser method: " + name + ".");
            }

            return data;
        }

        static Parser_Block start(Pattern_Source data)
        {
            return new Parser_Block
            {
                type = "block",
                expressions = data.patterns[1]
            };
        }

        static Parser_Symbol create_symbol(Pattern_Source data)
        {
            return new Parser_Symbol
            {
                type = "symbol",
                name = (string)data.patterns[2],
                expression = (Parser_Item)data.patterns[6]
            };
        }

        static object expression(Pattern_Source data, Match match)
        {
            if (data.patterns.Length < 2)
                return data.patterns[0];

            var rep_match = (Repetition_Match)match;
            string op = rep_match.dividers[0].matches[1].get_data().text;

            if (op == "|")
            {
                var function_name = data.patterns.Last();
                data = ;
                var block = ((Parser_Block) function_name);

                return new Parser_Function_Call
                    {
                        type = "function",
                        name = block.expressions[0].name,
                        inputs = data.patterns.Take(data.patterns.Length - 1).ToArray()
                    };
            }
            else
            {
                return new Parser_Function_Call
                    {
                        type = "function",
                        name = op,
                        inputs = data
                    };
            }
        }

        //static object method (object data) {
        //    return {
        //type= "function",
        //"name"= data.patterns[1],
        //"inputs"= []
        //}
        //}

        static Parser_Condition condition(Pattern_Source data)
        {
            return new Parser_Condition
                {
                    type = "condition",
                    first = (Parser_Item)data.patterns[0],
                    op = ((object[])data.patterns[2])[0].ToString(),
                    //"op"= Std.string(function_map[data.patterns[2][0]]),
                    second = (Parser_Item)data.patterns[4]
                };
        }

        static object optional_block(Pattern_Source data)
        {
            return data.patterns[1];
        }

        static object conditions(Pattern_Source data, Match match)
        {
            var rep_match = (Repetition_Match)match;
            if (data.Length > 1)
            {
                string symbol = rep_match.dividers[0].matches[1].get_data().text;
                string divider = null;
                switch (symbol)
                {

                    case "&&": divider = "and";
                        break;

                    case "||": divider = "or";
                        break;

                    default: throw new Exception("Invalid condition group joiner: " + symbol + ".");
                }
                return new Parser_Conditions
                    {
                        type = "conditions",
                        conditions = (Parser_Condition[])data,
                        mode = divider
                    };
            }
            else
            {
                //throw new Exception("Not implemented.");
                return data.patterns[0];
            }
        }

        static object condition_block(Pattern_Source data)
        {
            return data.patterns[2];
        }

        static Parser_If if_statement(Pattern_Source data)
        {
            return new Parser_If
                {
                    type = "if",
                    condition = (Parser_Condition)data.patterns[2],
                    action = (Parser_Item)data.patterns[4]
                };
        }

        static Parser_Constraint create_constraint(Pattern_Source data)
        {
            return new Parser_Constraint
            {
                type = "specific_constraint",
                path = (Parser_Item)data.patterns[0],
                expression = (Parser_Item)data.patterns[4]
            };
        }

        //static object create_node (object data) {
        //  object result = {
        //          type= "create_node",
        //          trellis= data.patterns[2]
        //  };

        //  if (data.patterns[3] != null && data.patterns[3].Count > 0) {
        //          result.block = data.patterns[3][0];
        //    //result.set = data.patterns[4][0];
        //  }

        //  return result;
        //}

        static object reference(Pattern_Source data, Repetition_Match match)
        {
            var dividers = match.dividers.Select((d) => d.matches[0].get_data());
            //
            //if (data.Count == 1) {
            //return {
            //type= "reference",
            //path= [ data.patterns[0] ]
            //}
            //}

            var tokens = new List<Parser_Item>();
            throw new Exception();
            //data.patterns[0].type == "array"
            //? data.patterns[0]
            //: new Parser_Reference {
            //    type= "reference",
            //    name= (string) data.patterns[0]
            //}			
            /*
              for (var i = 1; i <data.Length; ++i) {
                  var token = data.patterns[i];
                  var divider = dividers[i - 1];
                  if (divider == ".") {
                      tokens.Add({
                          type= "reference",
                          name= token
                      });
                  }
                  else if (divider == "|") {
                      tokens.Add({
                          type= "function",
                          name= token
                      });
                  }
                  else {
                      throw new Exception("Invalid divider= " + divider);
                  }
              }
            */
            return new Parser_Block
            {
                type = "path",
                expressions = tokens.ToArray()
            };
        }

        static object long_block(Pattern_Source data)
        {
            return new Parser_Block
            {
                type = "block",
                expressions = (Parser_Item[])data.patterns[2]
            };
        }

        static object set_property(Pattern_Source data)
        {
            var result = new Parser_Assignment
            {
                type = "set_property",
                path = (Parser_Item)data.patterns[0],
                expression = (Parser_Item)data.patterns[6],
            };

            var modifier = (object[])data.patterns[4];

            if (modifier.Length > 0)
                result.modifier = modifier[0].ToString();

            return result;
        }

        static object set_weight(Pattern_Source data)
        {
            return new Parser_Set_Weight
                {
                    type = "weight",
                    weight = (float)data.patterns[0],
                    statement = (Parser_Item)data.patterns[4]
                };
        }

        static object value(object data)
        {
            return new Parser_Literal
            {
                type = "literal",
                value = data
            };
        }

        static object new_scope(Pattern_Source data)
        {
            return new Parser_Scope
            {
                type = "new_scope",
                path = (string[])data.patterns[0],
                expression = (Parser_Item)data.patterns[2]
            };
        }

        static object constraint_block(Pattern_Source data)
        {
            return data.patterns[2];
        }

        static object constraint(Pattern_Source data)
        {
            return new Parser_Constraint
            {
                type = "constraint",
                reference = (Parser_Item)data.patterns[0],
                //op= Std.string(function_map[data.patterns[2]]),
                op = (string)data.patterns[2],
                expression = (Parser_Item)data.patterns[4],
                lambda = (Parser_Lambda) ((object[])data.patterns[5])[0]
            };
        }

        static object array_expression(Pattern_Source data)
        {
            return new Parser_Block
            {
                type = "array",
                expressions = (Parser_Item[])data.patterns[2]
            };
        }

        static object lambda_expression(Pattern_Source data)
        {
            return new Parser_Lambda
            {
                type = "lambda",
                parameters = (string[])data.patterns[1],
                expressions = ((Parser_Block)data.patterns[3]).expressions
            };
        }

        static object parameters(Pattern_Source data)
        {
            return data.patterns[2];
        }

        static object function_scope(Pattern_Source data)
        {
            return new Parser_Function_Scope
            {
                type = "function_scope",
                expression =(Parser_Item) data.patterns[0],
                lambda =(Parser_Lambda) data.patterns[1]
            };
        }

    }
}