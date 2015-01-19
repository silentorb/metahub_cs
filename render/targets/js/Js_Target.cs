using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative;
using metahub.imperative.schema;
using metahub.logic.schema;
using metahub.imperative.types;

namespace metahub.render.targets.js
{
    class Js_Target : Target
    {
        public Js_Target(Railway railway, Overlord overlord)
            :base(railway, overlord)
        {
            
        }

        override public void run(string output_folder)
        {
            var output = "";
            foreach (var dungeon in overlord.dungeons)
            {
                if (dungeon.is_external || (dungeon.is_abstract && dungeon.is_external))
                    continue;

                //Console.WriteLine(dungeon.realm.name + "." + dungeon.name);

                var space = Generator.get_namespace_path(dungeon.realm);

                line_count = 0;
                create_class_file(dungeon);
            }

            Utility.create_file(output_folder + "/" + "lib.js", output);

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
                    return render_region(space.realm, () => render_statements(space.expressions));

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
                    var state = (Statement)statement;
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

    }
}
