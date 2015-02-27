using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative;
using metahub.imperative.code;
using metahub.logic.schema;
using metahub.logic.nodes;

namespace metahub.logic
{
    public class Logician
    {
        public static Dictionary<string, string> inverse_operators = new Dictionary<string, string>
            {
                {"=", "!="},
                {"!=", "="},

                {">", "<="},
                {"<", ">="},
                {">=", "<"},
                {"<=", ">"},

                {"+", "-"},
                {"-", "+"},
                {"*", "/"},
                {"/", "*"}
            };

        public static string get_inverse_operator(string op, bool is_assignment)
        {
            if (is_assignment && op == "=")
                return "=";

            return inverse_operators[op];
        }

        public List<Constraint> constraints = new List<Constraint>();
        public List<Function_Node> functions = new List<Function_Node>(); 
        public Dictionary<string, Constraint_Group> groups = new Dictionary<string, Constraint_Group>();
        public bool needs_hub = false;
        public Railway railway;

        public Logician(Railway railway)
        {
            this.railway = railway;
        }

//        public Constraint create_constraint(Node first, Node second, string op, Lambda lambda, Logic_Scope scope)
//        {
//            var constraint = new Constraint(first, second, op, lambda) { constraint_scope = scope.constraint_scope };
//
//            var tie = Parse.get_end_tie(constraint.first);
//            if (tie != null)
//                tie.constraints.Add(constraint);
//
//            //if (!scope.is_map)
//                constraints.Add(constraint);
//
//            return constraint;
//        }

        public Function_Node call(string name, IEnumerable<Node> inputs, Logic_Scope scope = null)
        {
            var result = new Function_Node(name, inputs);
            if (scope != null && scope.parent_function != null)
            {
                scope.parent_function.children.Add(result);
            }
            else
            {
                functions.Add(result);
                
            }
            return result;
        }

        public void analyze()
        {
            analyze(functions);
        }

        public void analyze(List<Function_Node> funcs)
        {
            foreach (var function in funcs)
            {
                if (function.is_circular)
                    needs_hub = true;

                analyze(function.children);
            }

        }
    }
}
