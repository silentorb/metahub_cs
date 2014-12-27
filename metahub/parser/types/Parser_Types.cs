using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.metahub.parser.types
{

    public class Parser_Item
    {
        public string type;
    }

    public class Parser_Block : Parser_Item
    {
        public Parser_Item[] expressions;
    }

    public class Parser_Reference : Parser_Item
    {
        public string name;
    }

    public class Parser_Symbol : Parser_Item
    {
        public string name;
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
        public string name;
        public object[] inputs;
    }

    public class Parser_Constraint : Parser_Item
    {
        public Parser_Item path;
        public Parser_Item expression;
    }
}

