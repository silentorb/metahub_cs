using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.meta.types;
using metahub.schema;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Function_Call;
using Namespace = metahub.imperative.types.Namespace;
using Parameter = metahub.imperative.types.Parameter;
using Property_Expression = metahub.imperative.types.Property_Expression;
using Variable = metahub.imperative.types.Variable;

namespace metahub.imperative.schema {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Dungeon
{
	public Rail rail;
	public List<Expression> code;
	public Region region;
	public Trellis trellis;
	Dictionary<string, string> inserts;
	Dictionary<string, List<Expression>> blocks = new Dictionary<string, List<metahub.imperative.types.Expression>>();
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
		
		foreach (var tie in rail.all_ties.Values) {
			Lair lair = new Lair(tie, this);
			lairs[tie.name] = lair;
		}
	}
	
	void map_additional () {
		if (!region.trellis_additional.ContainsKey(trellis.name))
			return;	
			
		var map = (Dictionary<string, object>)region.trellis_additional[trellis.name];

		if (map.ContainsKey("inserts"))
			inserts = map["inserts"];
	}
	
	public void generate_code1 () {
		Class_Definition definition = new Class_Definition(rail, new List<Expression>());
		code = new List<Expression>();
		var zone = create_zone(code);
		zone.divide("..pre");
		var mid = zone.divide(null, new List<Expression> {
			new Namespace(region, new List<Expression> { definition })
    });
		zone.divide("..post");

		blocks["/"] = definition.expressions;
	}

	public void generate_code2 () {
		var statements = blocks["/"];
		statements.Add(generate_initialize());

		foreach (var tie in rail.all_ties.Values) {
			if (tie.type == Kind.list) {
				List_Code.common_functions(tie, imp);
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
				concat_block(path, lines.Select((s)=> new Insert(s)));
			}
		}
	}

    public List<Expression> get_block(string path)
    {
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

	public void concat_block (string path, IEnumerable<Expression> code) {
		var block = get_block(path);
	    block.AddRange(code);
	}

    public Zone create_zone(List<Expression> target = null)
    {
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

			Function_Definition result = new Function_Definition("set_" + tie.tie_name, this, new List<Parameter>{
				new Parameter("value", tie.get_signature())
			}, new List<Expression>());

		var zone = create_zone(result.expressions);
		var pre = zone.divide(tie.tie_name + "_set_pre");

		var mid = zone.divide(null, new List<Expression> {
			new Flow_Control("if", new Condition("==", new List<Expression> {
					new Property_Expression(tie), new Variable("value")
            }),
				new List<Expression>{
					new Statement("return")
                }),
			new Assignment(new Property_Expression(tie), "=", new Variable("value"))
		});

		if (tie.type == Kind.reference && tie.other_tie != null) {
			if (tie.other_tie.type == Kind.reference) {
				mid.Add(new Property_Expression(tie,
					new Function_Call("set_" + tie.other_tie.tie_name, new List<object> {new Self()})
				));
			}
			else {
				mid.Add(new Property_Expression(tie,
					new Function_Call(tie.other_tie.tie_name + "_add", new List<object>{ new Self(), new Self()}))
				);
			}
		}

		var post = zone.divide(tie.tie_name + "_set_post");

		if (tie.has_set_post_hook) {
			post.Add(new Function_Call(tie.get_setter_post_name(), new List<object> {
					new Variable("value")
			}));
		}

		return result;
	}

	public Function_Definition generate_initialize () {
		var block = new List<Expression>();
		blocks["initialize"] = block;
		if (rail.parent != null) {
			block.Add(new Parent_Class(
				new Function_Call("initialize")
			));
		}
		
		foreach (var lair in lairs.Values) {
			lair.customize_initialize(block);
		}

		if (rail.hooks.ContainsKey("initialize_post")) {
			block.Add(new Function_Call("initialize_post"));
		}

		return new Function_Definition("initialize", this, new List<Parameter>(), block);
	}
	
	public void post_analyze (Expression expression) {
		switch(expression.type) {
			
			case Expression_Type.space:
                post_analyze_many(((Namespace)expression).expressions);
		        break;

            case Expression_Type.class_definition:
				post_analyze_many(((Class_Definition)expression).expressions);
                break;

            case Expression_Type.function_definition:
				post_analyze_many(((Function_Definition)expression).expressions);
                break;

            case Expression_Type.flow_control:
				post_analyze_many(((Flow_Control)expression).condition.expressions);
                post_analyze_many(((Flow_Control)expression).children);
                break;

            case Expression_Type.function_call:
				var definition = (Function_Call)expression;
				//trace("func", definition.name);
				if (definition.is_platform_specific && !used_functions.ContainsKey(definition.name))
					used_functions[definition.name] = new Used_Function(definition.name, definition.is_platform_specific);
				
				foreach (var arg in definition.args) {
                    throw new Exception("Not implemented.");
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
	
	public void post_analyze_many (List<Expression> expressions) {
		foreach (var expression in expressions) {
			post_analyze(expression);
		}
	}
}}