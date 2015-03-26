//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using parser;
//
//namespace runic.parser
//{
//
//    public class Parser_Bootstrap : Parser_Context
//    {
//        public Parser_Bootstrap(Definition definition)
//            : base(definition)
//        {
//        }
//
//        public override object perform_action(string name, Pattern_Source data, Match match)
//        {
//            if (data.name == null)
//                data.name = name;
//
//            var type = match.pattern.name;
//            if (data.type == null)
//                data.type = type;
//
//            switch (type)
//            {
//                case "group":
//                    var repetition = (Repetition) match.matches[0].pattern;
//                    data.type = repetition.divider.name == "spaces"
//                        ? "and"
//                        : "or";
//                    return data;
//
//                case "option":
//                    return data;
////                case "regex":
////                    data = data.patterns[1];
////                    data.type = type;
////                    return data;
//                    //                default:
//                    //                    throw new Exception("Invalid parser method: " + name + ".");
//            }
//
//            return data;
//        }
//    }
//}
