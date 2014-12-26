using metahub.imperative.Imp;
using metahub.imperative.types.*;
using metahub.logic.schema.Rail;
using metahub.logic.schema.Region;
using metahub.logic.schema.Tie;
using metahub.schema.Trellis;
using metahub.schema.Kind;
using metahub.imperative.code.List;

namespace a {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Dungeon
{
	public Rail rail;
	public List<Expression> code;
	public Region region;
	public Trellis trellis;
	Object inserts;
	Dictionary<string, List<metahub.imperative.types.Expression>> blocks = new Dictionary<string, List<metahub.imperative.types.Expression>>();
	List<Zone> zones = new List<Zone>();
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
		
		foreach (var tie in rail.all_ties) {
			Lair lair = new Lair(tie, this);
			lairs[tie.name] = lair;
		}
	}
	
	void map_additional () {
		if (!region.trellis_additional.ContainsKey(trellis.name))
			return;	
			
		Rail_Additional map = region.trellis_additional[trellis.name];

		if (map.ContainsKey("inserts"))
			inserts = map.inserts;
	}
	
	public void generate_code1 () {
		Class_Definition definition = new Class_Definition(rail, []);
		code = [];
		var zone = create_zone(code);
		zone.divide("..pre");
		var mid = zone.divide(null, [
			new Namespace(region, [ definition ])
		]);
		zone.divide("..post");

		blocks["/"] = definition.expressions;
	}

	public void generate_code2 () {
		var statements = blocks["/"];
		statements.Add(generate_initialize());

		foreach (var tie in rail.all_ties) {
			if (tie.type == Kind.list) {
				List.common_functions(tie, imp);
			}
			else {
				var definition = generate_setter(tie);
				if (definition != null)
					statements.Add(definition);
			}
		}

		if (inserts != null) {
			foreach (var path in inserts.Keys) {
				List<string> lines = inserts[path];
				concat_block(path, cast lines.map((s)=> new Insert(s)));
			}
		}
	}

	public void get_block (string path) {
		if (!blocks.ContainsKey(path)) {
		}

		if (!blocks.ContainsKey(path))
			throw new Exception("Invalid rail block: " + path + ".");

		return blocks[path];
	}

	public void add_to_block (string path, Expression code) {
		var block = get_block(path);
		block.Add(code);
	}

	public void concat_block (string path, List<Expression> code) {
		var block = get_block(path);
		foreach (var expression in code) {
			block.Add(expression);
		}
	}

	public void create_zone (target = null) {
		Zone zone = new Zone(target, blocks);
		zones.Add(zone);
		return zone;
	}

	public void flatten () {
		foreach (var zone in zones) {
			zone.flatten();
		}
	}
	
	Function_Definition generate_setter (Tie tie) {
		if (!tie.has_setter())
			return null;

			Function_Definition result = new Function_Definition("set_" + tie.tie_name, this, [
				new Parameter("value", tie.get_signature())
			], []);

		var zone = create_zone(result.expressions);
		var pre = zone.divide(tie.tie_name + "_set_pre");

		var mid = zone.divide(null, [
			new Flow_Control("if", new Condition("==", [
					new Property_Expression(tie), new Variable("value")
				]),
				[
					new Statement("return")
			]),
			new Assignment(new Property_Expression(tie), "=", new Variable("value"))
		]);
		if (tie.type == Kind.reference && tie.other_tie != null) {
			if (tie.other_tie.type == Kind.reference) {
				mid.Add(new Property_Expression(tie,
					new Function_Call("set_" + tie.other_tie.tie_name, [new Self()]))
				);
			}
			else {
				mid.Add(new Property_Expression(tie,
					new Function_Call(tie.other_tie.tie_name + "_add", [new Self(), new Self()]))
				);
			}
		}

		var post = zone.divide(tie.tie_name + "_set_post");

		if (tie.has_set_post_hook) {
			post.Add(new Function_Call(tie.get_setter_post_name(), [
					new Variable("value")
				]
			));
		}

		return result;
	}

	public Function_Definition generate_initialize () {
		var block = [];
		blocks["initialize"] = block;
		if (rail.parent != null) {
			block.Add(new Parent_Class(
				new Function_Call("initialize")
			));
		}
		
		foreach (var lair in lairs) {
			lair.customize_initialize(block);
		}

		if (rail.hooks.ContainsKey("initialize_post")) {
			block.Add(new Function_Call("initialize_post"));
		}

		return new Function_Definition("initialize", this, [], block);
	}
	
	public void post_analyze (Expression expression) {
		switch(expression.type) {
			
			case Expression_Type.space:
				Namespace ns = cast expression;
				post_analyze_many(ns.expressions);

			case Expression_Type.class_definition:
				Class_Definition definition = cast expression;
				post_analyze_many(definition.expressions);

			case Expression_Type.function_definition:
				Function_Definition definition = cast expression;
				post_analyze_many(definition.expressions);

			case Expression_Type.flow_control:
				Flow_Control definition = cast expression;
				post_analyze_many(definition.condition.expressions);
				post_analyze_many(definition.children);

			case Expression_Type.function_call:
				Function_Call definition = cast expression;
				//trace("func", definition.name);
				if (definition.is_platform_specific && !used_functions.ContainsKey(definition.name))
					used_functions[definition.name] = new Used_Function(definition.name, definition.is_platform_specific);
				
				foreach (var arg in definition.args) {
					if (arg.ContainsKey("type"))
						post_analyze(arg);
				}

			case Expression_Type.assignment:
				Assignment definition = cast expression;
				post_analyze(definition.expression);

			case Expression_Type.declare_variable:
				Declare_Variable definition = cast expression;
				post_analyze(definition.expression);
				
			//case Expression_Type.property:
				//Property_Expression property_expression = cast expression;
				//result = property_expression.tie.tie_name;

			//case Expression_Type.instantiate:

			default:
				// Do nothing.  This is a gentler function than most MetaHub expression processors.
		}
	}
	
	public void post_analyze_many (List<Expression> expressions) {
		foreach (var expression in expressions) {
			post_analyze(expression);
		}
	}
}}