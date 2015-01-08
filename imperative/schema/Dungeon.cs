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
using Variable = metahub.imperative.types.Variable;

namespace metahub.imperative.schema
{
    public class Dungeon
    {
        public string name;
        public Realm realm;
        public Rail rail;
        public Dungeon parent;
        public List<Expression> code;
        public Region region;
        public Trellis trellis;
        Dictionary<string, string[]> inserts;
        Dictionary<string, Block> blocks = new Dictionary<string, Block>();
        public List<Function_Definition> functions = new List<Function_Definition>();
        public Overlord overlord;
        List<Imp> imps = new List<Imp>(); 
        public Dictionary<string, Portal> lairs = new Dictionary<string, Portal>();
        public Dictionary<string, Used_Function> used_functions = new Dictionary<string, Used_Function>();
        public Dictionary<string, Dependency> dependencies = new Dictionary<string, Dependency>();

        public Dungeon(Rail rail, Overlord overlord, Realm realm)
        {
            this.rail = rail;
            this.overlord = overlord;
            this.realm = realm;

            name = rail.name;
            region = rail.region;
            trellis = rail.trellis;

            map_additional();

            foreach (var tie in rail.all_ties.Values)
            {
                Portal lair = new Portal(tie, this);
                lairs[tie.name] = lair;
            }
        }

        public void initialize()
        {
            if (rail.parent != null)
                parent = overlord.get_dungeon(rail.parent);
        }

        public Dependency add_dependency(Dungeon dungeon)
        {
            if (dungeon == this)
                return null;

            if (!dependencies.ContainsKey(dungeon.name))
                dependencies[dungeon.name] = new Dependency(dungeon);

            return dependencies[dungeon.name];
        }

        public Dependency add_dependency(Rail dependency_rail)
        {
            if (dependency_rail == null)
                return null;

            var dungeon = overlord.get_dungeon(dependency_rail);
            if (dungeon == null)
                return null;

            return add_dependency(dungeon);
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
            generate_initialize(statements.scope);

            foreach (var tie in rail.all_ties.Values)
            {
                if (tie.type == Kind.list)
                {
                    List_Code.common_functions(tie, overlord, statements.scope);
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
                    if (tokens.Length > 1)
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

        private Function_Definition generate_setter(Tie tie, Scope scope)
        {
            if (!tie.has_setter())
                return null;

            var function_scope = new Scope(scope);
            var value = function_scope.create_symbol("value", tie.get_signature());
            var parameters = new List<Parameter>
                {
                    new Parameter(value)
                };

            Function_Definition result = new Function_Definition("set_" + tie.tie_name, this,
                    parameters, new List<Expression>());

            var block = create_block("set_" + tie.tie_name, new Scope(function_scope), result.expressions);

            var pre = block.divide("pre");

            var mid = block.divide(null, new List<Expression> {
			new Flow_Control(Flow_Control_Type.If, new Operation("==", new List<Expression> {
					new Tie_Expression(tie), new Variable(value)
            }),
				new List<Expression>{
					new Statement("return")
                }),
			new Assignment(new Tie_Expression(tie), "=", new Variable(value))
		});

            if (tie.type == Kind.reference && tie.other_tie != null)
            {
                var origin = function_scope.create_symbol("origin", new Signature(Kind.reference));
                parameters.Add(new Parameter(origin, new Null_Value()));
                var dungeon = overlord.get_dungeon(tie.rail);
   
                if (tie.other_tie.type == Kind.reference)
                {
                    mid.add(new Tie_Expression(tie,
                        new Function_Call("set_" + tie.other_tie.tie_name, new List<Expression> { new Self(dungeon) })
                    ));
                }
                else
                {
                    mid.add(new Flow_Control(Flow_Control_Type.If, new Operation("!=", new List<Expression>
                {
                    new Variable(origin), new Variable(value)
                }), new List<Expression> {
                        new Tie_Expression(tie,
                        new Function_Call("add_" + tie.other_tie.tie_name, new List<Expression> { new Self(dungeon), new Self(dungeon) }))
                    
                    }));
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

        public Imp generate_initialize(Scope scope)
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

            //return new Function_Definition("initialize", this, new List<Parameter>(), expressions);
            return spawn_imp("initialize", new List<Parameter>(), expressions);
        }

        public void analyze()
        {
            if (rail.parent != null && !rail.parent.trellis.is_abstract)
            {
                add_dependency(overlord.get_dungeon(rail.parent)).allow_partial = false;
            }

            foreach (var lair in lairs.Values)
            {
                var tie = lair.tie;
                if (tie.other_tie != null && !tie.other_rail.trellis.is_abstract)
                {
                    add_dependency(tie.other_rail);
                }
            }

            analyze_expressions(code);
        }

        void analyze_expression(Expression expression)
        {
            switch (expression.type)
            {
                case Expression_Type.space:
                    analyze_expressions(((Namespace)expression).expressions);
                    break;

                case Expression_Type.class_definition:
                    analyze_expressions(((Class_Definition)expression).expressions);
                    break;

                case Expression_Type.function_definition:
                    analyze_expressions(((Function_Definition)expression).expressions);
                    break;

                case Expression_Type.operation:
                    analyze_expressions(((Operation)expression).expressions);
                    break;

                case Expression_Type.flow_control:
                    analyze_expression(((Flow_Control)expression).expression);
                    analyze_expressions(((Flow_Control)expression).children);
                    break;

                case Expression_Type.function_call:
                    var definition = (Function_Call)expression;
                    if (definition.is_platform_specific && !used_functions.ContainsKey(definition.name))
                        used_functions[definition.name] = new Used_Function(definition.name, definition.is_platform_specific);

                    analyze_expressions(definition.args);
                    break;

                case Expression_Type.property_function_call:
                    var property_function = (Property_Function_Call)expression;
                    analyze_expressions(property_function.args);
                    break;

                case Expression_Type.assignment:
                    analyze_expression(((Assignment)expression).expression);
                    break;

                case Expression_Type.declare_variable:
                    var declare_variable = (Declare_Variable) expression;
                    add_dependency(declare_variable.symbol.signature.rail);
                    analyze_expression(declare_variable.expression);
                    break;

                case Expression_Type.property:
                    var property_expression =(Tie_Expression) expression;
                    add_dependency(property_expression.tie.other_rail);
                    break;

                case Expression_Type.variable:
                    var variable_expression = (Variable)expression;
                    add_dependency(variable_expression.symbol.signature.rail);
                    break;

                case Expression_Type.iterator:
                    var iterator = (Iterator) expression;
                    analyze_expression(iterator.expression);
                    analyze_expressions(iterator.children);
                    break;
            }

            if (expression.child != null)
                analyze_expression(expression.child);
        }

        void analyze_expressions(IEnumerable<Expression> expressions)
        {
            foreach (var expression in expressions)
            {
                analyze_expression(expression);
            }
        }

        public Function_Definition add_function(string function_name, List<Parameter> parameters, Signature return_type = null)
        {
            var definition = new Function_Definition(
                function_name, this, parameters, new List<Expression>(), return_type);

            var block = get_block("class_definition");
            block.add(definition);
            definition.scope = new Scope(block.scope);

            return definition;
        }

        public Imp spawn_imp(string imp_name, List<Parameter> parameters = null, List<Expression> expressions = null, Signature return_type = null, Portal portal = null)
        {
            var imp = new Imp(imp_name, this, portal)
                {
                    parameters = parameters ?? new List<Parameter>(),
                    expressions = expressions ?? new List<Expression>(),
                    return_type = return_type ?? new Signature(Kind.none)
                };
            imps.Add(imp);

            var definition = new Function_Definition(imp);

            var block = get_block("class_definition");
            block.add(definition);
            definition.scope = imp.scope = new Scope(block.scope);

            return imp;
        }

        public Imp summon_imp(string imp_name)
        {
            return imps.First(i => i.name == imp_name);
        }
    }
}