using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative;
using imperative.code;
using metahub.logic.schema;
using metahub.logic.nodes;
using metahub.schema;

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
        public Schema railway;

        public Logician(Schema railway)
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


        void initialize_root_functions(Namespace space)
        {
            space.add_functions(new List<Function_Info> {

			new Function_Info("contains", new List<Signature> {
				new Signature(Kind.Bool, new []
				    {
				        new Signature(Kind.list),
				        new Signature(Kind.reference)
				    })
			}),

			new Function_Info("count", new List<Signature> {
				new Signature(Kind.Int, new [] { new Signature(Kind.list)})
			}),
			
			new Function_Info("cross", new List<Signature> {
                new Signature(Kind.none, new []
                    {
                        new Signature(Kind.list),
                        new Signature(Kind.none, new []
                            {
                                new Signature(Kind.reference),
                                new Signature(Kind.reference)
                            }), 
                    })
			}),
			
			new Function_Info("distance", new List<Signature> {
                new Signature(Kind.Float, new []
                    {
                        new Signature(Kind.reference),
                        new Signature(Kind.Float, new []
                            {
                                new Signature(Kind.reference),
                                new Signature(Kind.reference),
                            }), 
                    })
			}),

            new Function_Info("first", new List<Signature> {
				new Signature(Kind.reference, new [] { new Signature(Kind.list)})
			}),

            new Function_Info("map", new List<Signature> {
                new Signature(Kind.none, new []
                    {
                        new Signature(Kind.list),
                        new Signature(Kind.none, new []
                            {
                                new Signature(Kind.reference),
                                new Signature(Kind.reference), 
                            }), 
                    })
			}),
		});
        }
    }
}
