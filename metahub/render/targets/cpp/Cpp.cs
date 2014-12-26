using System;
using System.Collections.Generic;
using metahub.imperative;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.meta.types;
using metahub.schema;
using Function_Call = metahub.meta.types.Function_Call;
using Parameter = metahub.meta.types.Parameter;
using Variable = metahub.meta.types.Variable;

namespace metahub.render.targets.cpp
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class External_Header {
	public string name;
	public bool is_standard;
	
	public External_Header(string name, bool is_standard = false)
	{
		this.name = name;
		this.is_standard = is_standard;
	}
}
public class Cpp : Target{

	Region current_region;
	Rail current_rail;
	List<Dictionary<string, Signature>> scopes = new List<Dictionary<string, Signature>>();
	Dictionary<string, Signature> current_scope;
	Dungeon current_dungeon;

    private static Dictionary<string, string> types = new Dictionary<string, string>
        {
            {"string", "std,,string"},
            {"int", "int"},
            {"bool", "bool"},
            {"float", "float"},
            {"none", "void"}
        };

	public Cpp(Railway railway, Imp imp)
:base(railway, imp) {
	}

	override public void run (string output_folder) {
		foreach (var region in railway.regions.Values){
			foreach (var rail in region.rails.Values) {
				if (rail.is_external)
					continue;

				//trace(rail.space.fullname);
				var space = Generator.get_namespace_path(rail.region);
				var dir = output_folder + "/" + space.Join("/");
				Utility.create_folder(dir);

				line_count = 0;
				var dungeon = imp.get_dungeon(rail);
				create_header_file(dungeon, space, dir);
				create_class_file(dungeon, space, dir);
			}
		}
	}

	override void generate_rail_code (Dungeon dungeon) {
		var rail = dungeon.rail;
		var root = dungeon.get_block("/");
		List<Tie> references = new List<Tie>();
		List<Tie> scalars = new List<Tie>();
		foreach (var tie in rail.core_ties) {
			if (tie.type == Kind.reference && !tie.is_value)
				references.Add(tie);
			else if (tie.type != Kind.list)
				scalars.Add(tie);
		}

		Function_Definition func = new Function_Definition(rail.rail_name, dungeon, [],
			references.map((tie)=> new Assignment(
				new Property_Expression(tie), "=", new Null_Value())				
			)
			.concat(scalars.map((tie)=> new Assignment(
				new Property_Expression(tie), "=", new Literal(tie.get_default_value(), tie.get_signature())))
			)
		);
		func.return_type = null;
		root.Add(func);
		func = new Function_Definition("~" + rail.rail_name, dungeon, new List<imperative.types.Parameter>(), 
		new List<Expression>()	//references.map((tie)=> new Function_Call("SAFE_DELETE",
				//[new Property_Reference(tie)])
			//)
		);
		func.return_type = null;
		root.Add(func);
	}

	void push_scope () {
		current_scope = new Dictionary<string, Signature>();
		scopes.Add(current_scope);
	}

	void pop_scope () {
		scopes.pop();
		current_scope = scopes[scopes.Count - 1];
	}

	void create_header_file (Dungeon dungeon, string space, string dir) {
		var rail = dungeon.rail;
		List<External_Header> headers = [ new External_Header("stdafx") ];

		foreach (var d in rail.dependencies) {
			var dependency = d.rail;
			if (!d.allow_ambient)
				headers.Add(new External_Header(dependency.source_file));
		}

		render = new Renderer();
		var result = line("#pragma once")
		+ render_includes(headers) + newline()
		+ render_outer_dependencies(rail)
		+ render_region(rail.region, ()=> newline() + render_inner_dependencies(rail) + class_declaration(dungeon);
		);
		Utility.create_file(dir + "/" + rail.name + ".h", result);
	}

	void create_class_file (Dungeon dungeon, string space, string dir) {
		var rail = dungeon.rail;
		scopes = [];
		List<External_Header> headers = [ new External_Header("stdafx"), new External_Header(rail.source_file) ];
		foreach (var d in rail.dependencies) {
			var dependency = d.rail;
			if (dependency != rail.parent && dependency.source_file != null) {
				headers.Add(new External_Header(dependency.source_file));
			}
		}
		
		foreach (var func in dungeon.used_functions) {
			if (func.name == "rand" && func.is_platform_specific) {
				if (!has_header(headers, "stdlib"))
					headers.Add(new External_Header("stdlib", true));
			}
		}
		
		render = new Renderer();
		var result = render_includes(headers) + newline()
		+ render_statements(dungeon.code);

		Utility.create_file(dir + "/" + rail.name + ".cpp", result);
	}

	string render_statements (List<object> statements, string glue = "") {
		return statements.Select((s)=> render_statement(s)).join(glue);
	}

	void render_statement (object statement) {
		Expression_Type type = statement.type;
		switch(type) {
			case Expression_Type.space:
				return render_region(statement.region, ()=>{
					return render_statements(statement.expressions);
				});

			case Expression_Type.class_definition:
				return class_definition(statement.rail, statement.expressions);

			case Expression_Type.function_definition:
				return render_function_definition(statement);

			case Expression_Type.flow_control:
				return render_if(statement);

			case Expression_Type.function_call:
				return line(render_function_call(statement, null) + ";");

			case Expression_Type.assignment:
				return render_assignment(statement);

			case Expression_Type.declare_variable:
				return render_variable_declaration(statement);

			case Expression_Type.statement:
				Statement s = statement;
				return line(s.name + ";");

			case Expression_Type.insert:
				Insert insert = statement;
				return line(insert.code);

			default:
				return line(render_expression(statement) + ";");
				//throw new Exception("Unsupported statement type: " + statement.type + ".");
		}
	}

	string render_variable_declaration (Declare_Variable declaration) {
		var first = render_signature(declaration.signature) + " " + declaration.name;
		if (declaration.expression != null)
			first += " = " + render_expression(declaration.expression);

		current_scope[declaration.name] = declaration.signature;
		return line(first + ";");
	}

    private class Temp
    {
        public Region region;
        public List<Rail> dependencies;
    }

	string render_outer_dependencies (Rail rail) {
		bool lines = false;
		var result = "";
		Dictionary<string, Temp> regions = new Dictionary<string, Temp>();

		foreach (var d in rail.dependencies.Values) {
			var dependency = d.rail;
			if (d.allow_ambient && dependency.region != rail.region) {
				if (!regions.ContainsKey(dependency.region.name))
				{
				    regions[dependency.region.name] = new Temp
				        {
				            region = dependency.region,
				            dependencies = new List<Rail>()
				        };
				}
				regions[dependency.region.name].dependencies.Add(dependency);
				lines = true;
			}
		}

		foreach (var r in regions) {
			result += render_region(r.region, ()=> r.dependencies.map((d)=> line("class " + d.rail_name + ";"))
					.Join("")
			);
		}

		if (result.Length > 0)
			result += newline();

		return result;
	}

	string render_inner_dependencies (Rail rail) {
		bool lines = false;
		var result = "";
		foreach (var d in rail.dependencies.Values) {
			var dependency = d.rail;
			if (d.allow_ambient && dependency.region == rail.region) {
				result += line("class " + get_rail_type_string(dependency) + ";");
				lines = true;
			}
		}

		if (result.Length > 0)
			result += newline();

		return result;
	}

	string class_declaration (Dungeon dungeon) {
		var rail = dungeon.rail;
		current_rail = rail;
		var result = "";
		var first = "class ";
		if (rail.class_export.Length > 0)
			first += rail.class_export + " ";

		first += rail.rail_name;
		if (rail.trellis.parent != null) {
			first += " : public " + render_rail_name(rail.parent);
		}

		result = line(first + " {")
		+ "public:" + newline();
		indent();

		foreach (var tie in rail.core_ties) {
			result += property_declaration(tie);
		}

		result += pad(render_function_declarations(dungeon))
		+ unindent().line("};");

		current_rail = null;
		return result;
	}

	string class_definition (Rail rail, List<object> statements) {
		current_rail = rail;
		var result = "";

		//result += pad(render_functions(rail));
		result += newline() + render_statements(statements, newline());
		unindent();

		current_rail = null;
		return result;
	}

	string render_region (Region region, String_Delegate action) {
		var space = Generator.get_namespace_path(region);
		var result = line("namespace " + space.Join("::") + " {");
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

	string render_rail_name (Rail rail) {
		if (rail.region != current_region)
			return render_region_name(rail.region) + "::" + rail.rail_name;

		return rail.rail_name;
	}

	string render_region_name (Region region) {
		var path = Generator.get_namespace_path(region);
		return path.Join("::");
	}

	string render_function_definition (Function_Definition definition) {
		var intro = (definition.return_type != null ? render_signature(definition.return_type) + " " : "")
		+ current_rail.rail_name + "::" + definition.name
		+ "(" + definition.parameters.Select(render_parameter).join(", ") + ")";
        
		return render_scope(intro, ()=>{
			foreach (var parameter in definition.parameters) {
				current_scope[parameter.name] = parameter.signature;
			}

		  return render_statements(definition.expressions);
		});
	}

	string render_parameter (Parameter parameter) {
		return render_signature(parameter.signature, true) + " " + parameter.name;
	}

	string render_function_declarations (Dungeon dungeon) {
		var declarations = [ ]
			.concat(dungeon.rail.stubs.map((s)=> line(s)));

		if (dungeon.rail.hooks.ContainsKey("initialize_post")) {
			declarations.Add(line("void initialize_post(); // Externally defined."));
		}

		foreach (var tie in dungeon.rail.all_ties) {
			if (tie.has_set_post_hook)
				declarations.Add(line("void " + tie.get_setter_post_name() + "(" + get_property_type_string(tie, true) +  " value);"));
		}

		//foreach (var tie in rail.all_ties) {
			//if (tie.has_setter())
				//declarations.Add(line(render_signature_old("set_" + tie.tie_name, tie) + ";"));
		//}

		foreach (var func in dungeon.functions) {
			declarations.Add(render_function_declaration(func));
		}

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

	string get_rail_type_string (Rail rail) {
		var name = rail.rail_name;
		if (rail.region.external_name != null)
			name = rail.region.external_name + "::" + name;
		else if (rail.region != current_region)
			name = rail.region.name + "::" + name;

		return name;
	}
	
	static bool has_header (List<External_Header> list , string name) {
		foreach (var header in list) {
			if (header.name == name)
				return true;
		}
		return false;
	}

	string get_property_type_string (Tie tie, bool is_parameter = false) {
		var other_rail = tie.other_rail;
		if (other_rail == null)
			return types[tie.property.type.ToString()];

		var other_name = get_rail_type_string(other_rail);
		if (tie.property.type == Kind.reference) {
			return tie.is_value ? is_parameter ? other_name + "&" : other_name :
					other_name + "*";
		}
		else {
			return "std::vector<" + other_name + ">";
		}
	}

	string property_declaration (Tie tie) {
		return line(get_property_type_string(tie) + " " +	tie.tie_name + ";");
	}

	string render_includes (List<External_Header> headers) {
		return headers.map((h)=>{
			return line(h.is_standard
				? "#include <" + h.name + ".h>"
				: "#include "" + h.name + ".h""
				); 
		} ).join("");
	}

	string render_signature_old (name, Tie tie) {
		var right = name + "(" + get_property_type_string(tie, true) + " value)";
		return "void " + right;
	}

	string render_function_declaration (Function_Definition definition) {
		return line((definition.return_type != null ? "virtual " : "")
		+ (definition.return_type != null ? render_signature(definition.return_type) + " " : "")
		+ definition.name
		+ "(" + definition.parameters.map(render_parameter).join(", ") + ");");

	}

	string render_signature (Signature signature, bool is_parameter = false) {
		if (signature.rail == null) {
			return signature.type == Kind.reference
				? "void*"
				: types[signature.type.to_string(]);
		}

		var name = get_rail_type_string(signature.rail);
		if (signature.type == Kind.reference) {
			return
			signature.is_value ? is_parameter ? name + "&" : name :
					name + "*";
		}
		else {
			return "std::vector<" + name + ">";
		}
	}

	public string render_block (string command, string expression, action) {
		var result = line(command + " (" + expression + ") {");
		indent();
		result += action();
		unindent();
		result += line("}");
		return result;
	}

	public string render_scope (string intro, action) {
		push_scope();
		var result = line(intro + " {");
		indent();
		result += action();
		unindent();
		result += line("}");
		pop_scope();
		return result;
	}

	public string render_scope2 (string intro, List<object> statements, bool minimal = false) {
		indent();
		var lines = line_count;
		var block = render_statements(statements);
		unindent();

		if (minimal) {
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

	string render_path (List<Tie> path) {
		return path.map((t)=> t.tie_name).join("->");
	}

	//string render_function_call (Function_Call statement) {
		//return line(statement.name + "();");
	//}

	string render_if (Flow_Control statement) {
		return render_scope2(
			statement.name + " (" + render_condition(statement.condition) + ")"
		, statement.children, statement.name == "if");
	}

	string render_condition (Condition condition) {
		return condition.expressions.map((c)=> render_expression(c)).join(" " + condition.op + " ");
	}

	string render_expression (Expression expression, Expression parent = null) {
		string result;
		switch(expression.type) {
			case Expression_Type.literal:
				return render_literal(expression);

			case Expression_Type.path:
				result = render_path_old(expression);

			case Expression_Type.property:
				Property_Expression property_expression = expression;
				result = property_expression.tie.tie_name;

			case Expression_Type.function_call:
				result = render_function_call(expression, parent);

			case Expression_Type.instantiate:
				result = render_instantiation(expression);

			case Expression_Type.self:
				result = "this";

			case Expression_Type.null_value:
				return "NULL";

			case Expression_Type.variable:
				Variable variable_expression = expression;
				if (find_variable(variable_expression.name) == null)
					throw new Exception("Could not find variable: " + variable_expression.name + ".");

				result = variable_expression.name;

			case Expression_Type.parent_class:
				result = current_rail.parent.rail_name;

			default:
				throw new Exception("Unsupported Node type: " + expression.type + ".");
		}

		if (expression.child != null) {
			result += get_connector(expression) + render_expression(expression.child, expression);
		}

		return result;
	}
	
	string render_literal (Literal expression) {
		var signature = expression.signature;
	    if (signature == null)
	        return expression.value.ToString();
			
		switch (signature.type) {
			case Kind.unknown:
				return expression.value.ToString();;
			
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
				return (bool)expression.value ? "true": "false";

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

	object get_signature (Expression expression) {
		switch (expression.type) {
			case Expression_Type.variable:
				Variable variable_expression = expression;
				return find_variable(variable_expression.name);

			case Expression_Type.property:
				Property_Expression property_expression = expression;
				return property_expression.tie;

			default:
				throw new Exception("Determining pointer is not yet implemented for Node type: " + expression.type + ".");
		}
	}

	bool is_pointer (object signature) {
		if (signature.type == null)
			throw "";
		return !signature.is_value && signature.type != Kind.list;
	}

	void get_connector (Expression expression) {
		if (expression.type == Expression_Type.parent_class)
			return "::";

		return is_pointer(get_signature(expression)) ? "->" : ".";
	}

	Signature find_variable (string name) {
		var i = scopes.Count;
		while (--i >= 0) {
			if (scopes[i].ContainsKey(name))
				return scopes[i][name];
		}

		return null;
	}

	string render_instantiation (Instantiate expression) {
		return "new " + get_rail_type_string(expression.rail) + "()";
	}

	string render_function_call (Function_Call expression, Expression parent) {
		if (expression.is_platform_specific) {
			//var args = Node.args.map((a)=> a).join(", ");

			switch (expression.name) {
				case "count":
					return "size()";

				case "add":
					var first = expression.args[0].name;
					var dereference = is_pointer(find_variable(first)) ? "*" : "";
					return "push_back(" + dereference + first + ")";

				case "rand":
					float min = (expression.args[0], Literal).value;
					float max = (expression.args[1], Literal).value;
					return "rand() % " + (max - min) + (min < 0 ? " - " + -min : " + " + min);						
					
				default:
					throw new Exception("Unsupported platform-specific function: " + expression.name + ".");
			}
		}

		return expression.name + "(" +
			expression.args.map((a)=> render_expression(a))
			.join(", ") + ")";
	}

	void render_path_old (Path expression) {
		Expression parent = null;
		var result = "";
		foreach (var child in expression.children) {
			if (parent != null)
				result += get_connector(parent);

			result += render_expression(child, parent);
			parent = child;
		}

		return result;
	}

	string render_assignment (Assignment statement) {
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