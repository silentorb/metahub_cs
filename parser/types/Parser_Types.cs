﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.parser.types
{
    /*
    public interface IParser_Item
    {
        string type { get; set; }
        string name { get; set; }
    }

    public class Parser_Item : IParser_Item
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class Parser_Block : Parser_Item
    {
        public Parser_Item[] expressions;
    }

    public class Parser_Lambda : Parser_Block
    {
        public string[] parameters;
    }

    public class Parser_Symbol : Parser_Item
    {
        public Parser_Item expression;
    }

    public class Parser_Condition : Parser_Item
    {
        public Parser_Item first;
        public Parser_Item second;
        public string op;
    }

    public class Parser_Conditions : Parser_Item
    {
        public Parser_Condition[] conditions;
        public string op;
        public string mode;
    }

    public class Parser_If : Parser_Item
    {
        public Parser_Condition condition;
        public Parser_Item action;
    }

    public class Parser_Function_Call : Parser_Item
    {
        public Pattern_Source[] inputs;
    }

    public class Parser_Constraint : Parser_Item
    {
        public Parser_Item path;
        public Parser_Item reference;
        public Parser_Item expression;
        public string op;
        public Parser_Lambda lambda;
    }

    public class Parser_Assignment : Parser_Constraint
    {
        public string modifier;
    }

    public class Parser_Set_Weight : Parser_Item
    {
        public float weight;
        public Parser_Item statement;
    }

    public class Parser_Literal : Parser_Item
    {
        public object value;
    }

    public class Parser_Scope : Parser_Item
    {
        public string[] path;
        public Parser_Item expression;
    }

    public class Parser_Function_Scope : Parser_Item
    {
        public Parser_Lambda lambda;
        public Parser_Item expression;
    }
     * */
}
