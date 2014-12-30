using System;
using System.Collections.Generic;
using metahub.logic.types;

namespace metahub.imperative.types
{

    /**
     * @author Christopher W. Johnson
     */
    public class Condition
    {
        public string op;
        public List<Expression> expressions;

        public Condition(string op, List<Expression> expressions)
        {
            this.op = op;
            this.expressions = expressions;
            //if (expressions[0].type == Expression_Type.path)
                //throw new Exception();
        }
    }
}