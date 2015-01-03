using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.logic.types;
using metahub.schema;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Function_Call;
using Namespace = metahub.imperative.types.Namespace;
using Parameter = metahub.imperative.types.Parameter;
using Property_Expression = metahub.imperative.types.Property_Expression;
using Variable = metahub.imperative.types.Variable;

namespace metahub.imperative.schema
{
    public class Dungeon
    {
        public Rail rail;
        public List<Expression> code;
        public Region region;
        public Trellis trellis;
        Dictionary<string, string[]> inserts;
        Dictionary<string, Block> blocks = new Dictionary<string, Block>();
        public List<Function_Definition> functions = new List<Function_Definition>();
        public Imp imp;
        public Dictionary<string, Lair> lairs = new Dictionary<string, Lair>();
        public Dictionary<string, Used_Function> used_functions = new Dictionary<string, Used_Function>();

        public Dungeon(Rail rail, Imp imp)
        {
            this.rail = rail;
            this.imp = imp;
            this.region = rail.region;
            trellis = rail.trellis;

            map_additional();

            foreach (var tie in rail.all_ties.Values)
            {
                Lair lair = new Lair(tie, this);
                lairs[tie.name] = lair;
            }
        }

        void map_additional()
        {
            if (!region.trellis_additional.ContainsKey(trellis.name))
                return;

            var map = region.trellis_additional[trellis.name];

            if (map.inserts != null)
                inserts = map.inserts;
        }

        public void generate_code1()
        {
            var root_scope = new Scope();
            code = new List<Expression>();
            var root = create_block("root", root_scope, code);
            root.divide("pre");

            var class_expressions = new List<Expression>();
            create_block("class_definition", root_scope, class_expressions);

            root.divide(null, new List<Expression> {
			    new Namespace(region, new List<Expression> { 
                    new Class_Definition(rail, class_expressions)
                })
            });

            root.divide("post");
        }

        public void generate_code2()
        {
            var statements = blocks["class_definition"];
            statements.add(generate_initialize(statements.scope));

            foreach (var tie in rail.all_ties.Values)
            {
                if (tie.type == Kind.list)
                {
                    List_Code.common_functions(tie, imp, statements.scope);
                }
                else
                {
                    var definition = generate_setter(tie, statements.scope);
                    if (definition != null)
                        statements.add(definition);
                }
            }

            if (inserts != null)
            {
                foreach (var path in inserts.Keys)
                {
                    var lines = inserts[path];
                    var tokens = path.Split('.');
                    if(tokens.Length > 1)
                        get_block(tokens[0]).add_many(tokens[1], lines.Select(s => new Insert(s)));
                    else
                        get_block(tokens[0]).add_many(lines.Select(s => new Insert(s)));
                }
            }
        }

        public Block create_block(string path, Scope scope, List<Expression> expressions = null)
        {
            var block = new Block(path, scope, this, expressions);
            blocks[path] = block;
            return block;
        }

        public bool has_block(string path)
        {
            return blocks.ContainsKey(path);
        }

        public Block get_block(string path)
        {
            if (!blocks.ContainsKey(path))
                throw new Exception("Invalid rail block: " + path + ".");

            return blocks[path];
        }

        //public void add_to_block(string path, Expression code)
        //{
        //    var block = get_block(path);
        //    block.Add(code);
        //}

        //public void concat_block(string path, IEnumerable<Expression> code)
        //{
        //    var block = get_block(path);
        //    block.AddRange(code);
        //}

        //public Zone create_zone(Block target)
        //{
        //    Zone zone = new Zone(target, this);
        //    zones.Add(zone);
        //    return zone;
        //}

        public void flatten()
        {
            foreach (var block in blocks.Values)
            {
                block.flatten();
            }
        }

        Function_Definition generate_setter(Tie tie, Scope scope)
        {
            if (!tie.has_setter())
                return null;

            var function_scope = new Scope(scope);
            var value = function_scope.create_symbol("value", tie.get_signature());

            Function_Definition result = new Function_Definition("set_" + tie.tie_name, this, new List<Parameter>{
				new Parameter(value)
			}, new List<Expression>());

            var block = create_block("set_" + tie.tie_name, new Scope(function_scope), result.expressions);

            var pre = block.divide("pre");

            var mid = block.divide(null, new List<Expression> {
			new Flow_Control(Flow_Control_Type.If, new Condition("==", new List<Expression> {
					new Property_Expression(tie), new Variable(value)
            }),
				new List<Expression>{
					new Statement("return")
                }),
			new Assignment(new Property_Expression(tie), "=", new Variable(value))
		});

            if (tie.type == Kind.reference && tie.other_tie != null)
            {
                if (tie.other_tie.type == Kind.reference)
                {
                    mid.add(new Property_Expression(tie,
                        new Function_Call("set_" + tie.other_tie.tie_name, new List<Expression> { new Self() })
                    ));
                }
                else
                {
                    mid.add(new Property_Expression(tie,
                        new Function_Call(tie.other_tie.tie_name + "_add", new List<Expression> { new Self(), new Self() }))
                    );
                }
            }

            var post = block.divide("post");

            if (tie.has_set_post_hook)
            {
                post.add(new Function_Call(tie.get_setter_post_name(), new List<Expression> {
					new Variable(value)
			    }));
            }

            return result;
        }

        public Function_Definition generate_initialize(Scope scope)
        {
            var expressions = new List<Expression>();
            var block = create_block("initialize", scope, expressions);
            block.divide("pre");
            block.divide("post");
            if (rail.parent != null)
            {
                block.add(new Parent_Class(
                    new Function_Call("initialize")
                ));
            }

            foreach (var lair in lairs.Values)
            {
                lair.customize_initialize(block);
            }

            if (rail.hooks.ContainsKey("initialize_post"))
            {
                block.add(new Function_Call("initialize_post"));
            }

            return new Function_Definition("initialize", this, new List<Parameter>(), expressions);
        }

        public void post_analyze(Expression expression)
        {
            switch (expression.type)
            {

                case Expression_Type.space:
                    post_analyze_many(((Namespace)expression).expressions);
                    break;

                case Expression_Type.class_definition:
                    post_analyze_many(((Class_Definition)expression).expressions);
                    break;

                case Expression_Type.function_definition:
                    post_analyze_many(((Function_Definition)expression).expressions);
                    break;

                case Expression_Type.condition:
                    post_analyze_many(((Condition)expression).expressions);
                    break;

                case Expression_Type.flow_control:
                    post_analyze(((Flow_Control)expression).expression);
                    post_analyze_many(((Flow_Control)expression).children);
                    break;

                case Expression_Type.function_call:
                    var definition = (Function_Call)expression;
                    //trace("func", definition.name);
                    if (definition.is_platform_specific && !used_functions.ContainsKey(definition.name))
                        used_functions[definition.name] = new Used_Function(definition.name, definition.is_platform_specific);

                    foreach (var arg in definition.args)
                    {
                        post_analyze(arg);
                        //throw new Exception("Not implemented.");
                        //if (arg.ContainsKey("type"))
                        //    post_analyze(arg);
                    }
                    break;

                case Expression_Type.assignment:
                    post_analyze(((Assignment)expression).expression);
                    break;

                case Expression_Type.declare_variable:
                    post_analyze(((Declare_Variable)expression).expression);
                    break;

                //case Expression_Type.property:
                //Property_Reference property_expression = Node;
                //result = property_expression.tie.tie_name;
            }
        }

        public void post_analyze_many(List<Expression> expressions)
        {
            foreach (var expression in expressions)
            {
                post_analyze(expression);
            }
        }
    }
}