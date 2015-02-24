using System;
using metahub.imperative.schema;


using metahub.schema;

namespace metahub.imperative.expressions
{
    public class Comment : Expression
    {
        public string text;
        public bool is_multiline;

        public Comment(string text)
            : base(Expression_Type.comment)
        {
            this.text = text;
            is_multiline = text.Contains("\n");
        }
    }
}