using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.types;
using metahub.parser;

namespace metahub.jackolantern.expressions
{
   public class Snippet : Expression
   {
       public string name;
       public Pattern_Source source;
       public string[] parameters;

       public Snippet(string name, Pattern_Source source, string[] parameters)
           : base(Expression_Type.snippet)
       {
           this.name = name;
           this.source = source;
           this.parameters = parameters;
       }
    }
}
