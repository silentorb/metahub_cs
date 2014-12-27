

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using metahub.Properties;
using metahub.imperative;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.meta;
using metahub.meta.types;
using metahub.parser;
using metahub.parser.types;
using metahub.render;
using metahub.schema;
using Namespace = metahub.schema.Namespace;
using Regex = System.Text.RegularExpressions.Regex;

namespace metahub {

    public delegate void Empty_Delegate();

public class Hub {
  public Schema schema;
  public Definition parser_definition;
	static Regex remove_comments = new Regex("/#[^\n]*");
	public Namespace metahub_namespace;
	public int max_steps = 100;

	static List<Hub> instances = new List<Hub>();

  public Hub() {
		instances.Add(this);
		schema = new Schema();

		metahub_namespace = schema.add_namespace("metahub");
  }

  public void load_parser () {
    Definition boot_definition = new Definition();
    boot_definition.load_parser_schema();
    Bootstrap context = new Bootstrap(boot_definition);

      var data = System.Text.Encoding.Default.GetString(Resources.metahub);//.Replace("\r", "");
      var result = context.parse(data, false);
		if (result.success) {
			var match = (Match)result;
			parser_definition = new Definition();
			parser_definition.load((Dictionary<string, Pattern_Source>) match.get_data());
		}
		else {
			throw new Exception("Error loading parser.");
		}
  }

  public void load_schema_from_file (string url, Namespace space, bool auto_identity = false)
  {
     var data = JsonConvert.DeserializeObject<Schema_Source>(File.ReadAllText(url));
    load_schema_from_object(data, space, auto_identity);
  }

	public void load_schema_from_string (string json, Namespace space, bool auto_identity = false) {
    var data = JsonConvert.DeserializeObject<Schema_Source>(File.ReadAllText(json));
    load_schema_from_object(data, space, auto_identity);
  }

	public void load_schema_from_object (Schema_Source data, Namespace space, bool auto_identity = false) {
    schema.load_trellises(data.trellises, new Load_Settings(space, auto_identity));
		if (data.is_external.HasValue && data.is_external.Value)
			space.is_external = true;

        //foreach (var key in data.Keys) {
        //    if (key == "trellises")
        //        continue;

        //    space.additional[key] = data[key];
        //}
  }

    public Node run_data(Parser_Item source, Railway railway)
    {
    Coder coder = new Coder(railway);
    return coder.convert_statement(source, null);
  }

	public Result parse_code (string code) {
		if (parser_definition == null) {
			load_parser();
		}
		MetaHub_Context context = new MetaHub_Context(parser_definition);
		var without_comments = remove_comments.Replace(code, "");
		//trace("without_comments", without_comments);
    return context.parse(without_comments);
	}

  //public void load_internal_trellises () {
    //    var functions = Utility.get_string_resource("inserts/core_nodes.json");
    //var data =JsonConvert.DeserializeObject<RootObject>(functions);
    

    //schema.load_trellises(data.trellises, new Load_Settings(metahub_namespace));
  //}

	public void generate (Parser_Item source, string target_name, string destination) {
		Imp imp = new Imp(this, target_name);
		var root = run_data(source, imp.railway);
		Generator generator = new Generator(this);
		var target = generator.create_target(imp, target_name);
		imp.run(root, target);
		generator.run(target, destination);
	}

}}