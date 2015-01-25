using System;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.logic.nodes;
using metahub.schema;

namespace metahub.imperative.types
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