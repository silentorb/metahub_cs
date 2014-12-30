using System.Collections.Generic;
using metahub.imperative;
using metahub.logic.types;

namespace metahub.logic.schema
{
    public class Constraint
    {
        public Node[] first;
        public Node[] second;
        public bool is_back_referencing = false;
        public string op;
        public List<Constraint> other_constraints = new List<Constraint>();
        public Lambda lambda;
        public Node[] caller;

        public Constraint(Node[] first, Node[] second, string op, Lambda lambda, Node[] caller)
        {
            this.op = op;
            this.first = first;
            this.second = second;
            this.lambda = lambda;
            this.caller = caller;
        }
    }
}