using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using metahub.logic.schema;
using metahub.logic.nodes;
using metahub.schema;
using parser;
using vineyard.Properties;
using Match = parser.Match;

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

        private static Definition parser_definition;
        public static Regex remove_comments = new Regex("#[^\n]*");

        public List<Constraint> constraints = new List<Constraint>();
        public List<Function_Node> functions = new List<Function_Node>(); 
        public Dictionary<string, Constraint_Group> groups = new Dictionary<string, Constraint_Group>();
        public bool needs_hub = false;
        public Schema schema;

        public Logician(Schema schema)
        {
            this.schema = schema;
            initialize_root_functions(schema);
        }

        public static void load_parser()
        {
            Definition boot_definition = new Definition();
            boot_definition.load_parser_schema();
            Bootstrap context = new Bootstrap(boot_definition);

            var result = context.parse(Resources.metahub_grammar, boot_definition.patterns[0], false);
            //Debug_Info.output(result);
            if (result.success)
            {
                var match = (Match)result;
                parser_definition = new Definition();
                parser_definition.load(match.get_data().dictionary);
            }
            else
            {
                throw new Exception("Error loading parser.");
            }
        }

        public Match parse_code(string code)
        {
            if (parser_definition == null)
            {
                load_parser();
            }
            MetaHub_Context context = new MetaHub_Context(parser_definition);
            var without_comments = remove_comments.Replace(code, "");
            var result = context.parse(without_comments, parser_definition.patterns[0]);
            if (!result.success)
            {
                Debug_Info.output(result);
                throw new Exception("Syntax Error at " + result.end.y + ":" + result.end.x + ". ");
            }

            return (Match)result;
        }

        public void apply_code(string code)
        {
            var source = parse_code(code);
            var data = source.get_data();
            Coder coder = new Coder(schema, this);
            coder.convert_statement(data, null);
        }

        public void apply_code(Match source)
        {
            var data = source.get_data();
            Coder coder = new Coder(schema, this);
            coder.convert_statement(data, null);
        }

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

        void initialize_root_functions(Schema space)
        {
            space.add_functions(new List<Function_Info> {

			new Function_Info("contains", new List<Signature> {
				new Signature(Kind.Bool, new []
				    {
				        new Signature(Kind.reference) { is_list = true },
				        new Signature(Kind.reference)
				    })
			}),

			new Function_Info("count", new List<Signature> {
				new Signature(Kind.Int, new [] { new Signature(Kind.reference) { is_list = true }})
			}),
			
			new Function_Info("cross", new List<Signature> {
                new Signature(Kind.none, new []
                    {
                        new Signature(Kind.reference) { is_list = true },
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
				new Signature(Kind.reference, new [] { new Signature(Kind.reference) { is_list = true }})
			}),

            new Function_Info("map", new List<Signature> {
                new Signature(Kind.none, new []
                    {
                        new Signature(Kind.reference) { is_list = true },
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
