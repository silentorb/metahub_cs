using System;
using System.Collections.Generic;
using System.Linq;
using metahub.Properties;
using metahub.imperative;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using Kind = metahub.schema.Kind;

namespace metahub.render.targets.cpp
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
    public class External_Header
    {
        public string name;
        public bool is_standard;

        public External_Header(string name, bool is_standard = false)
        {
            this.name = name;
            this.is_standard = is_standard;
        }
    }
    public class Cpp : Target
    {

        Region current_region;
        Rail current_rail;
        List<Dictionary<string, Signature>> scopes = new List<Dictionary<string, Signature>>();
        Dictionary<string, Signature> current_scope;
        Dungeon current_dungeon;

        private static Dictionary<string, string> types = new Dictionary<string, string>
        {
            {"string", "std::string"},
            {"int", "int"},
            {"bool", "bool"},
            {"float", "float"},
            {"none", "void"}
        };

        public Cpp(Railway railway, Overlord imp)
            : base(railway, imp)
        {
        }

        override public void run(string output_folder)
        {
            foreach (var region in railway.regions.Values)
            {
                foreach (var rail in region.rails.Values)
                {
                    if (rail.is_external || rail.trellis.is_abstract)
                        continue;

                    Console.WriteLine(rail.region.name + "." + rail.name);

                    var space = Generator.get_namespace_path(rail.region);
                    var dir = output_folder + "/" + space.join("/");
                    Utility.create_folder(dir);

                    line_count = 0;
                    var dungeon = imp.get_dungeon(rail);
                    create_header_file(dungeon, dir);
                    create_class_file(dungeon, dir);
                }
            }

            {
                var dir = output_folder + "/metahub";
                Utility.create_folder(dir);
                Utility.create_file(dir + "/" + "list.h", Resources.list_h);
            }
        }

        override public void generate_rail_code(Dungeon dungeon)
        {
            var rail = dungeon.rail;
            var root = dungeon.get_block("class_definition");
            List<Tie> references = new List<Tie>();
            List<Tie> scalars = new List<Tie>();
            foreach (var tie in rail.core_ties.Values)
            {
                if (tie.type == Kind.reference && !tie.is_value)
                    references.Add(tie);
                else if (tie.type != Kind.list)
                    scalars.Add(tie);
            }

            var block = references.Select((tie) => new Assignment(
                new Tie_Expression(tie), "=", new Null_Value()))
                .Union(scalars.Select((tie) => new Assignment(
                new Tie_Expression(tie), "=",
                new Literal(tie.get_default_value(),
                            tie.get_signature())))
                );

            Function_Definition func = new Function_Definition(rail.rail_name, dungeon, new List<imperative.types.Parameter>(),
                ((IEnumerable<Expression>)block).ToList());
            func.imp.return_type = null;
            root.add(func);
            func = new Function_Definition("~" + rail.rail_name, dungeon, new List<imperative.types.Parameter>(),
            new List<Expression>()	//references.map((tie)=> new Function_Call("SAFE_DELETE",
                //[new Property_Reference(tie)])
                //)
            );
            func.imp.return_type = null;
            root.add(func);
        }

        void push_scope()
        {
            current_scope = new Dictionary<string, Signature>();
            scopes.Add(current_scope);
        }

        void pop_scope()
        {
            scopes.RemoveAt(scopes.Count - 1);
            current_scope = scopes.Count > 0
                ? scopes[scopes.Count - 1]
                : null;
        }

        void create_header_file(Dungeon dungeon, string dir)
        {
            var rail = dungeon.rail;
            List<External_Header> headers = new List<External_Header> { new External_Header("stdafx") };

            foreach (var d in dungeon.dependencies.Values)
            {
                var dependency = d.dungeon;
                if (!d.allow_partial)
                    headers.Add(new External_Header(dependency.rail.source_file));
            }

            render = new Renderer();
            var result = line("#pragma once")
            + render_includes(headers) + newline()
            + render_outer_dependencies(dungeon)
            + render_region(rail.region, () => newline() + render_inner_dependencies(dungeon) + class_declaration(dungeon));
            Utility.create_file(dir + "/" + rail.name + ".h", result);
        }

        void create_class_file(Dungeon dungeon, string dir)
        {
            var rail = dungeon.rail;
            scopes = new List<Dictionary<string, Signature>>();
            List<External_Header> headers = new List<External_Header> { new External_Header("stdafx"), new External_Header(rail.source_file) };
            foreach (var d in dungeon.dependencies.Values)
            {
                var dependency = d.dungeon;
                if (dependency != dungeon.parent && dependency.rail.source_file != null)
                {
                    headers.Add(new External_Header(dependency.rail.source_file));
                }
            }

            foreach (var func in dungeon.used_functions.Values)
            {
                if (func.name == "rand" && func.is_platform_specific)
                {
                    if (!has_header(headers, "stdlib"))
                        headers.Add(new External_Header("stdlib", true));
                }
            }

            render = new Renderer();
            var result = render_includes(headers) + newline()
            + render_statements(dungeon.code);

            Utility.create_file(dir + "/" + rail.name + ".cpp", result);
        }

        string render_statements(IEnumerable<Expression> statements, string glue = "")
        {
            return statements.Select(render_statement).join(glue);
        }

        string render_statement(Expression statement)
        {
            Expression_Type type = statement.type;
            switch (type)
            {
                case Expression_Type.space:
                    var space = (Namespace)statement;
                    return render_region(space.region, () => render_statements(space.expressions));

                case Expression_Type.class_definition:
                    var definition = (Class_Definition)statement;
                    return class_definition(definition.rail, definition.expressions);

                case Expression_Type.function_definition:
                    return render_function_definition((Function_Definition)statement);

                case Expression_Type.flow_control:
                    return render_flow_control((Flow_Control)statement);

                case Expression_Type.iterator:
                    return render_iterator_block((Iterator)statement);

                case Expression_Type.function_call:
                    return line(render_function_call((Function_Call)statement, null) + ";");

                case Expression_Type.assignment:
                    return render_assignment((Assignment)statement);

                case Expression_Type.declare_variable:
                    return render_variable_declaration((Declare_Variable)statement);

                case Expression_Type.statement:
                    var state = (Statement) statement;
                    return line(state.name + (state.child != null
                        ? " " + render_expression(state.child)
                        : "") + ";");

                case Expression_Type.insert:
                    return line(((Insert)statement).code);

                default:
                    return line(render_expression(statement) + ";");
                //throw new Exception("Unsupported statement type: " + statement.type + ".");
            }
        }

        string render_variable_declaration(Declare_Variable declaration)
        {
            var first = render_signature(declaration.symbol.signature) + " " + declaration.symbol.name;
            if (declaration.expression != null)
                first += " = " + render_expression(declaration.expression);

            current_scope[declaration.symbol.name] = declaration.symbol.signature;
            return line(first + ";");
        }

        private class Temp
        {
            public Region region;
            public List<Dungeon> dependencies;
        }

        string render_outer_dependencies(Dungeon dungeon)
        {
            var rail = dungeon.rail;
            bool lines = false;
            var result = "";
            Dictionary<string, Temp> regions = new Dictionary<string, Temp>();

            foreach (var d in dungeon.dependencies.Values)
            {
                var dependency = d.dungeon;
                if (d.allow_partial && dependency.region != rail.region)
                {
                    if (!regions.ContainsKey(dependency.region.name))
                    {
                        regions[dependency.region.name] = new Temp
                            {
                                region = dependency.region,
                                dependencies = new List<Dungeon>()
                            };
                    }
                    regions[dependency.region.name].dependencies.Add(dependency);
                    lines = true;
                }
            }

            foreach (var r in regions.Values)
            {
                result += render_region(r.region, () => r.dependencies.Select(d => line("class " + d.rail.rail_name + ";"))
                        .join("")
                );
            }

            if (result.Length > 0)
                result += newline();

            return result;
        }

        string render_inner_dependencies(Dungeon dungeon)
        {
            bool lines = false;
            var result = "";
            foreach (var d in dungeon.dependencies.Values)
            {
                var dependency = d.dungeon;
                if (d.allow_partial && dependency.region == dungeon.region)
                {
                    result += line("class " + get_rail_type_string(dependency) + ";");
                    lines = true;
                }
            }

            if (result.Length > 0)
                result += newline();

            return result;
        }

        string class_declaration(Dungeon dungeon)
        {
            var rail = dungeon.rail;
            current_rail = rail;
            var result = "";
            var first = "class ";
            if (rail.class_export.Length > 0)
                first += rail.class_export + " ";

            first += rail.rail_name;
            if (rail.parent != null)
            {
                first += " : public " + render_rail_name(rail.parent);
            }

            result = line(first + " {")
            + "public:" + newline();
            indent();

            foreach (var portal in dungeon.core_portals.Values)
            {
                result += property_declaration(portal);
            }

            result += pad(render_function_declarations(dungeon))
            + unindent().line("};");

            current_rail = null;
            return result;
        }

        string class_definition(Rail rail, IEnumerable<Expression> statements)
        {
            current_rail = rail;
            var result = "";

            //result += pad(render_functions(rail));
            result += newline() + render_statements(statements, newline());
            unindent();

            current_rail = null;
            return result;
        }

        string render_region(Region region, String_Delegate action)
        {
            var space = Generator.get_namespace_path(region);
            var result = line("namespace " + space.join("::") + " {");
            current_region = region;
            indent();
            result += action()
            + unindent().line("}");

            current_region = null;
            return result;
        }

        //string render_functions (Rail rail) {
        //var result = "";
        //var definitions = [ render_initialize_definition(rail) ];
        //
        ////foreach (var tie in rail.all_ties) {
        ////var definition = render_setter(tie);
        ////if (definition.Count > 0)
        ////definitions.Add(definition);
        ////}
        ////
        //return definitions.join(newline());
        //}

        string render_rail_name(Rail rail)
        {
            if (rail.region != current_region)
                return render_region_name(rail.region) + "::" + rail.rail_name;

            return rail.rail_name;
        }

        string render_region_name(Region region)
        {
            var path = Generator.get_namespace_path(region);
            return path.join("::");
        }

        string render_function_definition(Function_Definition definition)
        {
            if (definition.is_abstract)
                return "";

            var intro = (definition.return_type != null ? render_signature(definition.return_type) + " " : "")
            + current_rail.rail_name + "::" + definition.name
            + "(" + definition.parameters.Select(render_definition_parameter).join(", ") + ")";

            return render_scope(intro, () =>
            {
                foreach (var parameter in definition.parameters)
                {
                    current_scope[parameter.symbol.name] = parameter.symbol.signature;
                }

                return render_statements(definition.expressions);
            });
        }

        string render_definition_parameter(Parameter parameter)
        {
            return render_signature(parameter.symbol.signature, true) + " " + parameter.symbol.name;
        }

        string render_declaration_parameter(Parameter parameter)
        {
            return render_signature(parameter.symbol.signature, true) + " " + parameter.symbol.name
                + (parameter.default_value != null 
                    ? " = " + render_expression(parameter.default_value) 
                    : ""
            );
        }

        string render_function_declarations(Dungeon dungeon)
        {
            var declarations = dungeon.rail.stubs.Select(line).ToList();

            if (dungeon.rail.hooks.ContainsKey("initialize_post"))
            {
                declarations.Add(line("void initialize_post(); // Externally defined."));
            }

            foreach (var tie in dungeon.rail.all_ties.Values)
            {
                if (tie.has_set_post_hook)
                    declarations.Add(line("void " + tie.get_setter_post_name() + "(" + get_property_type_string(tie, true) + " value);"));
            }

            //foreach (var tie in rail.all_ties) {
            //if (tie.has_setter())
            //declarations.Add(line(render_signature_old("set_" + tie.tie_name, tie) + ";"));
            //}

            declarations.AddRange(dungeon.functions.Select(render_function_declaration));

            return declarations.join("");
        }

        //string render_initialize_definition (Rail rail) {
        //var result = line("void " + rail.rail_name + "::initialize() {");
        //indent();
        //result += line(rail.parent != null
        //? rail.parent.rail_name + "::initialize();"
        //: ""
        //);
        //foreach (var tie in rail.all_ties) {
        //if (tie.property.type == Kind.list) {
        //foreach (var constraint in tie.constraints) {
        //result += Constraints.render_list_constraint(constraint, render, this);
        //}
        //}
        //}
        //if (rail.hooks.ContainsKey("initialize_post")) {
        //result += line("initialize_post();");
        //}
        //unindent();
        //return result + line("}");
        //}

        string get_rail_type_string(Rail rail)
        {
            var name = rail.rail_name;
            if (rail.region.external_name != null)
                name = rail.region.external_name + "::" + name;
            else if (rail.region != current_region)
                name = rail.region.name + "::" + name;

            return name;
        }

        string get_rail_type_string(Dungeon dungeon)
        {
            var name = dungeon.rail.rail_name;
            if (dungeon.region.external_name != null)
                name = dungeon.region.external_name + "::" + name;
            else if (dungeon.region != current_region)
                name = dungeon.region.name + "::" + name;

            return name;
        }

        static bool has_header(IEnumerable<External_Header> list, string name)
        {
            return list.Any(header => header.name == name);
        }

        string property_declaration(Portal portal)
        {
            return line(render_profession(portal.get_profession()) + " " + portal.name + ";");
        }

        string render_includes(IEnumerable<External_Header> headers)
        {
            return headers.Select(h => line(h.is_standard
                ? "#include <" + h.name + ".h>"
                : "#include \"" + h.name + ".h\""
            )).join("");
        }

        string render_function_declaration(Function_Definition definition)
        {
            return line((definition.return_type != null ? "virtual " : "")
            + (definition.return_type != null ? render_signature(definition.return_type) + " " : "")
            + definition.name
            + "(" + definition.parameters.Select(render_declaration_parameter).join(", ") + ")"
            + (definition.is_abstract ? " = 0;" : ";"));
        }

        string get_property_type_string(Tie tie, bool is_parameter = false)
        {
            return render_signature(tie.get_signature(), is_parameter);
        }

        string render_profession(Profession signature, bool is_parameter = false)
        {
            if (signature.dungeon == null)
            {
                return signature.type == Kind.reference
                    ? "void*"
                    : types[signature.type.ToString().ToLower()];
            }

            var name = signature.dungeon.is_abstract
                ? "void"
                : get_rail_type_string(signature.dungeon);

            if (signature.type == Kind.reference)
            {
                return
                signature.dungeon.is_value ? is_parameter ? name + "&" : name :
                        name + "*";
            }
            else
            {
                return "std::vector<" + (signature.dungeon.is_value
                ? name
                : name + "*")
                + ">";
            }
        }

        string render_signature(Signature signature, bool is_parameter = false)
        {
            if (signature.rail == null)
            {
                return signature.type == Kind.reference
                    ? "void*"
                    : types[signature.type.ToString().ToLower()];
            }

            var name = signature.rail.trellis.is_abstract
                ? "void"
                : get_rail_type_string(signature.rail);

            if (signature.type == Kind.reference)
            {
                return
                signature.rail.trellis.is_value ? is_parameter ? name + "&" : name :
                        name + "*";
            }
            else
            {
                return "std::vector<" + (signature.rail.trellis.is_value
                ? name
                : name + "*")
                + ">";
            }
        }

        public string render_block(string command, string expression, String_Delegate action)
        {
            var result = line(command + " (" + expression + ") {");
            indent();
            result += action();
            unindent();
            result += line("}");
            return result;
        }

        public string render_scope(string intro, String_Delegate action)
        {
            push_scope();
            var result = line(intro + " {");
            indent();
            result += action();
            unindent();
            result += line("}");
            pop_scope();
            return result;
        }

        public string render_scope2(string intro, List<Expression> statements, bool minimal = false)
        {
            indent();
            push_scope();
            var lines = line_count;
            var block = render_statements(statements);
            pop_scope();
            unindent();

            if (minimal)
            {
                minimal = line_count == lines + 1;
            }
            var result = line(intro + (minimal ? "" : " {"));
            result += block;
            result += line((minimal ? "" : "}"));
            return result;
        }

        //string render_setter (Tie tie) {
        //if (!tie.has_setter())
        //return "";

        //var result = line(render_signature_old("set_" + tie.tie_name, tie) + " {");
        //indent();
        //foreach (var constraint in tie.constraints) {
        //result += Constraints.render(constraint, render, this);
        //}
        //result +=
        //line("if (" + tie.tie_name + " == value)")
        //+ indent().line("return;")
        //+	unindent().newline()
        //+ line(tie.tie_name + " = value;");
        //if (tie.has_set_post_hook)
        //result += line(tie.get_setter_post_name() + "(value);");
        //
        //unindent();
        //result += line("}");
        //return result;
        //}

        //string render_path (List<Tie> path) {
        //    return path.Select(t => t.tie_name).join("->");
        //}

        //string render_function_call (Function_Call statement) {
        //return line(statement.name + "();");
        //}

        string render_flow_control(Flow_Control statement)
        {
            var expression = render_expression(statement.expression);

            return render_scope2(
                statement.flow_type.ToString().ToLower() + " (" + expression + ")"
            , statement.children, statement.flow_type == Flow_Control_Type.If);
        }

        string render_iterator_block(Iterator statement)
        {
            var parameter = statement.parameter;
            var it = parameter.scope.create_symbol("it", parameter.signature);
            var expression = render_iterator(it, statement.expression);

            var result = render_scope2(
                "for (" + expression + ")"
            , new List<Expression> { 
                    new Declare_Variable(parameter, new Insert("*" + it.name))
                }.Concat(statement.children).ToList()
            );
            return result;
        }

        string render_operation(Operation operation)
        {
            return operation.expressions.Select(c =>
                c.type == Expression_Type.operation && ((Operation)c).is_condition() == operation.is_condition()
                ? "(" + render_expression(c) + ")"
                : render_expression(c)
            ).join(" " + operation.op + " ");
        }

        string render_iterator(Symbol parameter, Expression expression)
        {
            var signature = Expression.get_end(expression).get_signature();
            var path_string = render_expression(expression);
            return
               render_signature(signature)
                + "::const_iterator " + parameter.name + " = "
                + path_string + ".begin(); " + parameter.name + " != "
                + path_string + ".end(); " + parameter.name + "++";
        }

        string render_expression(Expression expression, Expression parent = null)
        {
            string result;
            switch (expression.type)
            {
                case Expression_Type.literal:
                    return render_literal((Literal)expression);

                case Expression_Type.operation:
                    return render_operation((Operation)expression);

                case Expression_Type.path:
                    result = render_path_old((Path)expression);
                    break;

                case Expression_Type.property:
                    var tie_expression = (Tie_Expression) expression;
                    result = tie_expression.tie.tie_name;
                    if (tie_expression.index != null)
                        result += "[" + render_expression(tie_expression.index) + "]";

                    break;

                case Expression_Type.portal:
                    var portal_expression = (Portal_Expression)expression;
                    result = portal_expression.portal.name;
                    if (portal_expression.index != null)
                        result += "[" + render_expression(portal_expression.index) + "]";

                    break;

                case Expression_Type.function_call:
                    result = render_function_call((Function_Call)expression, parent);
                    break;

                case Expression_Type.property_function_call:
                    result = render_property_function_call((Property_Function_Call)expression, parent);
                    break;

                case Expression_Type.instantiate:
                    result = render_instantiation((Instantiate)expression);
                    break;

                case Expression_Type.self:
                    result = "this";
                    break;

                case Expression_Type.null_value:
                    return "NULL";

                case Expression_Type.variable:
                    var variable_expression = (Variable)expression;
                    //if (find_variable(variable_expression.symbol.name) == null)
                    //    throw new Exception("Could not find variable: " + variable_expression.symbol.name + ".");

                    result = variable_expression.symbol.name;
                    if (variable_expression.index != null)
                        result += "[" + render_expression(variable_expression.index) + "]";

                    break;

                case Expression_Type.parent_class:
                    result = current_rail.parent.rail_name;
                    break;

                case Expression_Type.insert:
                    result = ((Insert) expression).code;
                    break;

                default:
                    throw new Exception("Unsupported Expression type: " + expression.type + ".");
            }

            if (expression.child != null)
            {
                result += get_connector(expression) + render_expression(expression.child, expression);
            }

            return result;
        }

        string render_literal(Literal expression)
        {
            var signature = expression.signature;
            if (signature == null)
                return expression.value.ToString();

            switch (signature.type)
            {
                case Kind.unknown:
                    return expression.value.ToString(); ;

                case Kind.Float:
                    var result = expression.value.ToString();
                    return result.Contains(".")
                        ? result + "f"
                        : result;

                case Kind.Int:
                    return expression.value.ToString();

                case Kind.String:
                    return "\"" + expression.value + "\"";

                case Kind.Bool:
                    return (bool)expression.value ? "true" : "false";

                case Kind.reference:
                    if (!signature.rail.trellis.is_value)
                        throw new Exception("Literal expressions must be scalar values.");

                    if (expression.value != null)
                        return expression.value.ToString();

                    return render_rail_name(signature.rail) + "()";

                default:
                    throw new Exception("Invalid literal " + expression.value + " type " + expression.signature.type + ".");
            }
        }

        Signature get_signature(Expression expression)
        {
            switch (expression.type)
            {
                //case Expression_Type.variable:
                //    var variable_expression = (Variable)expression;
                //    return variable_expression.symbol.signature;

                //case Expression_Type.property:
                //    var property_expression = (Tie_Expression)expression;
                //    return property_expression.tie.get_signature();

                default:
                    return expression.get_signature();
                    //throw new Exception("Determining pointer is not yet implemented for Node type: " + expression.type + ".");
            }
        }

        bool is_pointer(Signature signature)
        {
            if (signature.type == null)
                throw new Exception();

            return !signature.is_value && signature.type != Kind.list;
        }
        
        bool is_pointer(Profession signature)
        {
            if (signature.type == null)
                throw new Exception();

            if (signature.dungeon == null)
                return false;

            return !signature.dungeon.is_value && signature.type != Kind.list;
        }

        string get_connector(Expression expression)
        {
            if (expression.type == Expression_Type.parent_class)
                return "::";

            if (expression.type == Expression_Type.portal)
                return is_pointer(expression.get_profession()) ? "->" : ".";

            return is_pointer(get_signature(expression)) ? "->" : ".";
        }

        //Signature find_variable (string name) {
        //    var i = scopes.Count;
        //    while (--i >= 0) {
        //        if (scopes[i].ContainsKey(name))
        //            return scopes[i][name];
        //    }

        //    return null;
        //}

        string render_instantiation(Instantiate expression)
        {
            return "new " + get_rail_type_string(expression.rail) + "()";
        }

        private string render_property_function_call(Property_Function_Call expression, Expression parent)
        {
            var ref_full = expression.reference != null
                ? render_expression(expression.reference) + get_connector(expression.reference)
                : "";

            var name = expression.tie.get_signature().type == Kind.list
                ? "add"
                : expression.function_type.ToString();

            return ref_full + name + "_" + expression.tie.tie_name + "("
                + expression.args.Select(e => render_expression(e)).join(", ")
                + ")";
        }

        string render_function_call(Function_Call expression, Expression parent)
        {
            var ref_string = expression.reference != null
               ? render_expression(expression.reference)
               : "";
            
            var ref_full = ref_string.Length > 0
                ? ref_string + get_connector(Expression.get_end(expression.reference))
                : "";

            if (expression.is_platform_specific)
            {
                //var args = Node.args.map((a)=> a).join(", ");
                switch (expression.name)
                {
                    case "count":
                        return ref_full + "size()";

                    case "add":
                        {
                            var first = render_expression(expression.args[0]);
                            //var dereference = is_pointer(expression.args.Last().get_signature()) ? "*" : "";
                            return ref_full + "push_back(" + first + ")";
                        }

                    case "contains":
                        {
                            var first = render_expression(expression.args[0]);
                            return "std::find(" + ref_full + "begin(), "
                                   + ref_full + "end(), " + first + ") != " + ref_full + "end()";
                        }

                    case "dist":
                        {
                            //var signature = expression.args[0].get_signature();
                            var first = render_expression(expression.args[0]);
                            //var dereference = is_pointer(signature) ? "*" : "";
                            return ref_full + "distance(" + first + ")";
                        }

                    case "last":
                        return ref_full + "back()";

                    case "pop":
                        return ref_full + "pop_back()";

                    case "remove":
                        {
                            var first = render_expression(expression.args[0]);
                            return ref_full + "erase(std::remove(" + ref_full + "begin(), "
                                + ref_full + "end(), " + first + "), " + ref_full + "end())";
                        }

                    case "rand":
                        float min = (float)((Literal)expression.args[0]).value;
                        float max = (float)((Literal)expression.args[1]).value;
                        return "rand() % " + (max - min) + (min < 0 ? " - " + -min : " + " + min);

                    default:
                        throw new Exception("Unsupported platform-specific function: " + expression.name + ".");
                }
            }

            return ref_full + expression.name + "(" +
                expression.args.Select(a => render_expression(a))
                .join(", ") + ")";
        }

        string render_path_old(Path expression)
        {
            Expression parent = null;
            var result = "";
            foreach (var child in expression.children)
            {
                if (parent != null)
                    result += get_connector(parent);

                result += render_expression(child, parent);
                parent = child;
            }

            return result;
        }

        string render_assignment(Assignment statement)
        {
            return line(render_expression(statement.target) + " " + statement.op + " " + render_expression(statement.expression) + ";");
        }

        //public object render_expression (Node Node, Scope scope) {
        //var type = Railway.get_class_name(Node);
        //trace("Node:", type);
        //
        //switch(type) {
        //case "Literal":
        //return render_literal(Node);
        //}
        //
        //throw new Exception("Cannot render Node " + type + ".");
        //}
        //
        //string render_literal (Literal Node) {
        //return Node.value;
        //}
    }
}