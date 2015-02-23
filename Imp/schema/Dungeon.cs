using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using metahub.imperative.types;
using metahub.schema;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Class_Function_Call;
using Namespace = metahub.imperative.types.Namespace;
using Parameter = metahub.imperative.types.Parameter;
using Variable = metahub.imperative.types.Variable;

namespace metahub.imperative.schema
{
    [DebuggerDisplay("dungeon ({name})")]
    public class Dungeon
    {
        public string name;
        public Realm realm;
        public Dungeon parent;
        public List<Expression> code;
        public Dictionary<string, string[]> inserts;
        Dictionary<string, Block> blocks = new Dictionary<string, Block>();
        public Overlord overlord;
        public Dictionary<string, Minion> minions = new Dictionary<string, Minion>();
        public Dictionary<string, Portal> all_portals = new Dictionary<string, Portal>();
        public Dictionary<string, Portal> core_portals = new Dictionary<string, Portal>();
        public Dictionary<string, Used_Function> used_functions = new Dictionary<string, Used_Function>();
        public Dictionary<string, Dependency> dependencies = new Dictionary<string, Dependency>();
        public bool is_external = false;
        public bool is_abstract = false;
        public bool is_value = false;
        public string source_file;
        public List<string> stubs = new List<string>();
        public Dictionary<string, object> hooks = new Dictionary<string, object>();
        public List<Dungeon> interfaces = new List<Dungeon>();
        public string class_export = "";
        private int id;
        private static int next_id = 1;
        public object default_value;

        public Dungeon(string name, Overlord overlord, Realm realm, Dungeon parent = null)
        {
            id = next_id++;
            this.name = name;
            this.overlord = overlord;
            this.realm = realm;
            this.parent = parent;
            realm.dungeons[name] = this;
            overlord.dungeons.Add(this);
            code = new List<Expression>();
            if (!is_external && source_file == null)
                source_file = realm.name + "/" + name;

            if (parent != null)
            {
                foreach (var portal in parent.all_portals.Values)
                {
                    all_portals[portal.name] = new Portal(portal, this);
                }
            }
        }

        public Dependency add_dependency(Dungeon dungeon)
        {
            if (dungeon == null || dungeon == this)
                return null;

            if (!dependencies.ContainsKey(dungeon.name))
                dependencies[dungeon.name] = new Dependency(dungeon);

            return dependencies[dungeon.name];
        }

        public void generate_code()
        {
            var root_scope = new Scope();
            code = new List<Expression>();
            var root = create_block("root", root_scope, code);
            root.divide("pre");

            var class_expressions = new List<Expression>();
            create_block("class_definition", root_scope, class_expressions);

            root.divide(null, new List<Expression> {
			    new Namespace(realm, new List<Expression> { 
                    new Class_Definition(this, class_expressions)
                })
            });

            root.divide("post");
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
            if (!has_block(path))
                throw new Exception("Dungeon " + name + " does not have a block named " + path + ".");
         
            return blocks[path];
        }

        public Portal add_portal(Portal portal)
        {
            if (core_portals.ContainsKey(portal.name))
                throw new Exception("Dungeon " + name + " already has a portal named " + portal.name + ".");

            portal.dungeon = this;
            all_portals[portal.name] = portal;
            core_portals[portal.name] = portal;
            return portal;
        }

        public bool has_portal(string portal_name)
        {
            return all_portals.ContainsKey(portal_name);
        }

        public Portal add_parent_portal(Portal portal)
        {
            if (all_portals.ContainsKey(portal.name))
                throw new Exception("Dungeon " + name + " already has a portal named " + portal.name + ".");

            portal.dungeon = this;
            all_portals[portal.name] = portal;
            return portal;
        }

        public Portal get_portal_or_null(string portal_name)
        {
            if (all_portals.ContainsKey(portal_name))
                return all_portals[portal_name];

            return null;
        }

        public void flatten()
        {
            foreach (var block in blocks.Values)
            {
                block.flatten();
            }
        }

        public void analyze()
        {
            //if (rail != null)
            //{
            //    if (rail.parent != null && !rail.parent.trellis.is_abstract)
            //    {
            //        add_dependency(overlord.get_dungeon(rail.parent)).allow_partial = false;
            //    }
            //}
            //else
            //{
            if (parent != null && !parent.is_abstract)
                add_dependency(parent).allow_partial = false;
            //}

            foreach (var @interface in interfaces)
            {
                add_dependency(@interface).allow_partial = false;
            }

            foreach (var portal in all_portals.Values)
            {
                if (portal.other_dungeon != null && !portal.other_dungeon.is_abstract)
                //if (tie.other_tie != null && !tie.other_rail.trellis.is_abstract)
                {
                    add_dependency(portal.other_dungeon);
                }
            }

            if (code == null)
                return;

            transform_expressions(code, null);
            analyze_expressions(code);
        }

        void transform_expression(Expression expression, Expression parent)
        {
            expression.parent = parent;
            if (overlord.target.transmuter != null)
                overlord.target.transmuter.transform(expression);

            switch (expression.type)
            {
                case Expression_Type.space:
                    transform_expressions(((Namespace)expression).expressions, expression);
                    break;

                case Expression_Type.class_definition:
                    transform_expressions(((Class_Definition)expression).expressions, expression);
                    break;

                case Expression_Type.function_definition:
                    transform_expressions(((Function_Definition)expression).expressions, expression);
                    break;

                case Expression_Type.operation:
                    transform_expressions(((Operation)expression).expressions, expression);
                    break;

                case Expression_Type.flow_control:
                    transform_expression(((Flow_Control)expression).expression, expression);
                    transform_expressions(((Flow_Control)expression).children, expression);
                    break;

                case Expression_Type.function_call:
                    var definition = (Function_Call)expression;
                    transform_expressions(definition.args, expression);
                    break;

                case Expression_Type.property_function_call:
                    var property_function = (Property_Function_Call)expression;
                    transform_expressions(property_function.args, expression);
                    break;

                case Expression_Type.assignment:
                    transform_expression(((Assignment)expression).expression, expression);
                    break;

                case Expression_Type.declare_variable:
                    var declare_variable = (Declare_Variable)expression;
                    transform_expression(declare_variable.expression, expression);
                    break;

                case Expression_Type.iterator:
                    var iterator = (Iterator)expression;
                    transform_expression(iterator.expression, expression);
                    transform_expressions(iterator.children, expression);
                    break;
            }

            if (expression.child != null)
                transform_expression(expression.child, expression);
        }

        void transform_expressions(IEnumerable<Expression> expressions, Expression parent_expression)
        {
            foreach (var expression in expressions)
            {
                transform_expression(expression, parent_expression);
            }
        }


        void analyze_expression(Expression expression)
        {
            overlord.target.analyze_expression(expression);

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
                    {
                        var definition = (Function_Call)expression;
                        analyze_expressions(definition.args);
                    }
                    break;

                case Expression_Type.platform_function:
                    {
                        var definition = (Platform_Function)expression;
                        if (!used_functions.ContainsKey(definition.name))
                            used_functions[definition.name] = new Used_Function(definition.name,
                                                                                true);

                        analyze_expressions(definition.args);
                    }
                    break;

                case Expression_Type.property_function_call:
                    var property_function = (Property_Function_Call)expression;
                    if (property_function.reference != null)
                        analyze_expression(property_function.reference);

                    analyze_expressions(property_function.args);
                    break;

                case Expression_Type.assignment:
                    {
                        var assignment = (Assignment)expression;
                        analyze_expression(assignment.target);
                        analyze_expression(assignment.expression);
                    }
                    break;

                case Expression_Type.declare_variable:
                    var declare_variable = (Declare_Variable)expression;
                    add_dependency(declare_variable.symbol.profession.dungeon);
                    analyze_expression(declare_variable.expression);
                    break;

                //case Expression_Type.property:
                //    var property_expression = (Tie_Expression)expression;
                //    add_dependency(property_expression.tie.other_rail);
                //    break;

                case Expression_Type.portal:
                    var portal_expression = (Portal_Expression)expression;
                    add_dependency(portal_expression.portal.other_dungeon);
                    break;

                case Expression_Type.variable:
                    var variable_expression = (Variable)expression;
                    if (variable_expression.symbol.profession.type == Kind.reference)
                        add_dependency(variable_expression.symbol.profession.dungeon);

                    break;

                case Expression_Type.iterator:
                    var iterator = (Iterator)expression;
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

        public Function_Definition add_function(string function_name, List<Parameter> parameters, Profession return_type = null)
        {
            var minion = spawn_minion(function_name, parameters, new List<Expression>(), return_type);
            return new Function_Definition(minion);
        }

        public Minion spawn_minion(string minion_name, List<Parameter> parameters = null, List<Expression> expressions = null, Profession return_type = null, Portal portal = null)
        {
            if (minions.ContainsKey(minion_name))
                throw new Exception("Dungeon " + name + " already contains an minion named " + minion_name + ".");

            var minion = new Minion(minion_name, this, portal)
                {
                    parameters = parameters ?? new List<Parameter>(),
                    expressions = expressions ?? new List<Expression>(),
                    return_type = return_type ?? new Profession(Kind.none)
                };

            minions[minion_name] = minion;

            var definition = new Function_Definition(minion);

            var block = get_block("class_definition");
            block.add(definition);
            definition.scope = minion.scope = new Scope(block.scope);

            return minion;
        }

        public bool has_minion(string minion_name, bool check_ancestors = false)
        {
            var result = minions.ContainsKey(minion_name);
            if (result || !check_ancestors || parent == null)
                return result;

            return parent.has_minion(minion_name, true);
        }

        public Minion summon_minion(string minion_name, bool check_ancestors = false)
        {
            if (minions.ContainsKey(minion_name))
                return minions[minion_name];

            if (!check_ancestors || parent == null)
                return null;

            return parent.summon_minion(minion_name, true);
        }

        public string get_available_name(string key, int start = 0)
        {
            var result = "";
            do
            {
                result = key;
                if (start != 0)
                    result += start;

                ++start;
            } while (has_minion(result) || all_portals.ContainsKey(result));

            return result;
        }
    }
}