﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using parser;

namespace runic.lexer
{


    public class Bootstrap_Legacy : Parser_Context
    {
        public Bootstrap_Legacy(Definition definition)
            : base(definition)
        {
        }

        public override object perform_action(string name, Pattern_Source data, Match match)
        {
            if (data.name == null)
                data.name = name;

            var type = match.pattern.name;
            switch (type)
            {
                case "string":
                case "regex":
                    data = data.patterns[1];
                    data.type = type;
                    return data;
                    //                default:
                    //                    throw new Exception("Invalid parser method: " + name + ".");
            }

            return data;
        }
    }
}
