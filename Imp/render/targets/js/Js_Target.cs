using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative;
using imperative.schema;

using imperative.expressions;

namespace metahub.render.targets.js
{
    public class Js_Target : Target
    {
        public Js_Target(Overlord overlord = null)
            : base(overlord)
        {

        }

        Realm current_realm;
        Dungeon current_dungeon;
        List<Dictionary<string, Profession>> scopes = new List<Dictionary<string, Profession>>();
        Dictionary<string, Profession> current_scope;

        override public void run(string output_folder)
        {
            var output = generate();
            Generator.create_file(output_folder + "/" + "lib.js", output);
        }

        public string generate()
        {
            var output = "";
            foreach (var dungeon in overlord.dungeons)
            {
                if (dungeon.is_external || (dungeon.is_abstract && dungeon.is_external))
                    continue;

                //Console.WriteLine(dungeon.realm.name + "." + dungeon.name);

                var space = Generator.get_namespace_path(dungeon.realm);

                line_count = 0;
                output += create_class_file(dungeon);
            }

            return output;
        }

        void push_scope()
        {
            current_scope = new Dictionary<string, Profession>();
            scopes.Add(current_scope);
        }

        void pop_scope()
        {
            scopes.RemoveAt(scopes.Count - 1);
            current_scope = scopes.Count > 0
                ? scopes[scopes.Count - 1]
                : null;
        }

        string create_class_file(Dungeon dungeon)
        {
            render = new Renderer();
            var result = render_statements(dungeon.code);

            return result;
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
                    return render_realm(space.realm, () => render_statements(space.children));

                case Expression_Type.class_definition:
                    var definition = (Class_Definition)statement;
                    return class_definition(definition.dungeon, definition.children);

                case Expression_Type.function_definition:
                    return render_function_definition((Function_Definition)statement);

                case Expression_Type.flow_control:
                    return render_flow_control((Flow_Control)statement);
                //
                //                case Expression_Type.iterator:
                //                    return render_iterator_block((Iterator)statement);
                //
                //                case Expression_Type.function_call:
                //                    return line(render_function_call((Function_Call)statement, null) + ";");
                //
                //                case Expression_Type.assignment:
                //                    return render_assignment((Assignment)statement);
                //
                //                case Expression_Type.declare_variable:
                //                    return render_variable_declaration((Declare_Variable)statement);
                //
                //                case Expression_Type.statement:
                //                    var state = (Statement)statement;
                //                    return line(state.name + (state.child != null
                //                        ? " " + render_expression(state.child)
                //                        : "") + ";");

                case Expression_Type.insert:
                    return line(((Insert)statement).code);

                default:
                    //                    return line(render_expression(statement) + ";");
                    throw new Exception("Unsupported statement type: " + statement.type + ".");
            }
        }

        public string full_dungeon_name(Dungeon dungeon)
        {
            return dungeon.realm.name + "." + dungeon.name;
        }

        string class_definition(Dungeon dungeon, IEnumerable<Expression> statements)
        {
            current_dungeon = dungeon;

            var result = line(full_dungeon_name(dungeon) + " = function() {}");
            var intro = full_dungeon_name(dungeon) + ".prototype =";
            result += render_scope(intro, () => render_statements(statements, newline()));

            current_dungeon = null;

            return result;
        }

        string render_function_definition(Function_Definition definition)
        {
            if (definition.is_abstract)
                return "";

            var intro = definition.name + ": function(" + definition.parameters.Select(p => p.symbol.name).join(", ") + ")";

            return render_scope(intro, () =>
            {
                foreach (var parameter in definition.parameters)
                {
                    current_scope[parameter.symbol.name] = parameter.symbol.profession;
                }

                return render_statements(definition.expressions);
            });
        }

        string render_realm(Realm realm, String_Delegate action)
        {
            var result = line("var " + realm.name + " = {}") + newline();

            current_realm = realm;
            result += action();
            current_realm = null;

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

        string render_flow_control(Flow_Control statement)
        {
            var expression = render_expression(statement.condition);

            return render_scope2(
                statement.flow_type.ToString().ToLower() + " (" + expression + ")"
            , statement.body, statement.flow_type == Flow_Control_Type.If);
        }

        private string render_expression(Expression expression, Expression parent = null)
        {
            throw new Exception("Not implemented.");
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

    }
}
