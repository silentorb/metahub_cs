using haxe.xml.Parser;
using metahub.imperative.Imp;
using metahub.logic.schema.Railway;
using metahub.meta.Coder;
using metahub.meta.types.Expression;
using metahub.parser.Definition;
using metahub.parser.Match;
using metahub.render.Generator;
using metahub.schema.Load_Settings;
using metahub.schema.Namespace;
using metahub.schema.Schema;
using metahub.schema.Trellis;
using metahub.schema.Property;

using metahub.schema.Kind;
using haxe.Json;

namespace b {
class Hub {
  public Schema schema;
  public metahub.parser.Definition parser_definition;
	static var remove_comments = ~/#[^\n]*/g;
	public Namespace metahub_namespace;
	public int max_steps = 100;

	static List<Hub> instances = new List<Hub>();

  public Hub() {
		instances.Add(this);
		schema = new Schema();

		metahub_namespace = schema.add_namespace("metahub");
    load_internal_trellises();

		//Math_Library math_library = new Math_Library();
		//metahub_namespace.children["Math"] = schema.add_namespace("Math", math_library);
		//math_library.load(this);
  }

	//public void add_change (INode node, int index, Object value, Context context, General_Port source = null) {
		//var i = queue.Count();
		//while (--i >= 0) {
			//if (queue[i].node == node) {
				//queue.splice(i, 1);
			//}
		//}
		//Pending_Change change = new Pending_Change(node, index, value, context, source);
		//queue.Add(change);
	//}
//
	//public void set_entry_node (Node node) {
		//if (entry_node == null)
			//entry_node = node;
	//}

	//public void unset_entry_node (Node node) {
		//if (entry_node == node)
			//entry_node = null;
	//}

	//public void run_change_queue (Node node) {
		//if (entry_node != node)
			//return;
//
		//int steps = 0;
		//while (queue.Count() > 0) {
			//var change = queue.shift();
			//change.run();
			//if (++steps > max_steps)
				//throw new Exception("Max steps of " + max_steps + " was reached.");
		//}
//
		//entry_node = null;
	//}

  private void load_parser () {
    metahub.parser.Definition boot_definition = new metahub.parser.Definition();
    boot_definition.load_parser_schema();
    metahub.parser.Bootstrap context = new metahub.parser.Bootstrap(boot_definition);
    var result = context.parse(metahub.Macros.insert_file_as_string("inserts/metahub.grammar"), false);
		if (result.success) {
			Match match = cast result;
			parser_definition = new Definition();
			parser_definition.load(match.get_data());
		}
		else {
			throw new Exception("Error loading parser.");
		}
  }

  //public Node create_node (Trellis trellis) {
		//var register = !trellis.is_value;
		//Node node = null;
		//var id = register ? ++node_count : 0;
		////if (id != 0)
			////trace("Creating " + trellis.name + " : " + id);
//
		//#if trace
			//Entry entry = new Entry("Create " + trellis.name + " " + id);
			//history.add(entry);
		//#end
//
		//foreach (var factory in node_factories) {
			//node = factory(this, id, trellis);
			//if (node != null)
				//break;
		//}
//
		//if (node == null)
			//throw new Exception("Could not find valid factory to create node of type " + trellis.name + ".");
//
		//if (register)
			//add_node(node);
//
		//new_nodes.Add(node);
//
		////node.initialize_values2();
		//if (!trellis.is_value) {
			//node.update_values();
			//node.update_on_create();
		//}
//
    //return node;
  //}

	//void add_node (Node node) {
		//if (nodes.ContainsKey(node.id))
			//throw new Exception("Node " + node.id + " already exists!");
//
		////foreach (var trellis in schema.trellises) {
			////trellis_nodes[trellis.name] = new List<Node>();
		////}
//
    //nodes[node.id] = node;
//
		//var tree = node.trellis.get_tree();
		//foreach (var t in tree) {
			//if (!trellis_nodes.ContainsKey(t.name))
				//trellis_nodes[t.name] = new List<Node>();
//
			//trellis_nodes[t.name].Add(node);
		//}
	//}
//
	//public void add_internal_node (INode node) {
    //internal_nodes.Add(node);
	//}
//
	//public Node get_node (int id) {
		//if (!nodes.ContainsKey(id))
		////if (id < 1 || id >= nodes.Count())
			//throw new Exception("There is no node with an id of " + id + ".");
//
		//return nodes[id];
	//}
//
  //public void get_node_count () {
    //return node_count;
  //}
//
	//public List<Node> get_nodes_by_trellis (Trellis trellis) {
		//return trellis_nodes.ContainsKey(trellis.name)
			//? trellis_nodes[trellis.name]
			//: [];
	//}

  public void load_schema_from_file (string url, Namespace namespace, bool auto_identity = false) {
    var data = Utility.load_json(url);
    load_schema_from_object(data, namespace, auto_identity);
  }

	public void load_schema_from_string (string json, Namespace namespace, bool auto_identity = false) {
    var data = Json.parse(json);
    load_schema_from_object(data, namespace, auto_identity);
  }

	public void load_schema_from_object (Object data, Namespace namespace, bool auto_identity = false) {
    schema.load_trellises(data.trellises, new Load_Settings(namespace, auto_identity));
		if (data.ContainsKey("is_external") && data["is_external"] == true)
			space.is_external = true;

		foreach (var key in data.Keys) {
			if (key == "trellises")
				continue;

			space.additional[key] = data[key];
		}
  }

  public Expression run_data (Object source, Railway railway) {
    Coder coder = new Coder(railway);
    return coder.convert_statement(source, null);
  }

  //public void run_code (string code) {
		//var result = parse_code(code);
		//if (!result.success) {
       //throw new Exception("Syntax Error at " + result.end.y + ":" + result.end.x);
		//}
    //metahub.parser.Match match = cast result;
		//var statement = run_data(match.get_data());
//
		////trace(graph_expressions(statement));
//
		////#if trace
			////history.new_tree();
		////#end
//
		////var port = statement.to_port(root_scope, new Group(null), null);
		////trace(graph_nodes(port.node));
//
		////#if trace
			////history.new_tree();
		////#end
//
		////port.get_node_value(new Empty_Context(this));
		////history.start_finished();
		////return port;
  //}

	public void parse_code (string code) {
		if (parser_definition == null) {
			load_parser();
		}
		metahub.parser.MetaHub_Context context = new metahub.parser.MetaHub_Context(parser_definition);
		var without_comments = remove_comments.replace(code, "");
		//trace("without_comments", without_comments);
    return context.parse(without_comments);
	}

  public void load_internal_trellises () {
		var functions = Macros.insert_file_as_string("inserts/core_nodes.json");
    var data = haxe.Json.parse(functions);
    schema.load_trellises(data.trellises, new Load_Settings(metahub_namespace));
  }

	public void generate (source, string target_name, string destination) {
		Imp imp = new Imp(this, target_name);
		var root = run_data(source, imp.railway);
		Generator generator = new Generator(this);
		var target = generator.create_target(imp, target_name);
		imp.run(root, target);
		generator.run(target, destination);
	}

	//public INode get_increment () {
		//if (interval_node == null) {
			//interval_node = new Block_Node(new Scope(this, root_scope_definition), new Group(null));
		//}
//
		//return interval_node;
	//}

	//public void connect_to_increment (General_Port port) {
		//var node = get_increment();
		//node.get_port(1).connect(port);
	//}
//
	//public void increment (int layer = 10000) {
		//var node = get_increment();
		//node.get_value(0, new Empty_Context(this));
		//new_nodes = [];
	//}

	//public static void get_node_label (INode node, General_Port port = null) {
		//Trellis trellis = Type.getClassName(Type.getClass(node)) == "metahub.schema.Trellis"
			//? cast node
			//: null;
//
		//return trellis != null && port != null
			//? trellis.properties[port.id].fullname()
			//: node.to_string();
	//}
//
	//public static string graph_nodes (INode node, int depth = 0, List<INode> used = null, General_Port port = null) {
		//if (used == null)
			//used = [];
//
		////int maximum_depth = 50;
		//Trellis trellis = Type.getClassName(Type.getClass(node)) == "metahub.schema.Trellis"
			//? cast node
			//: null;
//
		//var tabbing = " ";
		//var result = "";
		//var padding = "";
		//foreach (var i in 0...depth) {
			//padding += tabbing;
		//}
		//var label = get_node_label(node, port);
//
		//if (node.ContainsKey("id"))
			//label = "#" + node["id"] + " " + label;
//
		//result += padding + label + "\n";
//
		//if (used.indexOf(node) != -1 || trellis != null)
			//return result;
//
		//used.Add(node);
//
		////if (depth > maximum_depth) {
			////return result + padding + tabbing + "EXCEEDED MAXIMUM DEPTH OF " + maximum_depth + ".\n";
		////}
		//foreach (var i in 1...node.get_port_count()) {
			//var port = node.get_port(i);
			//int deeper = 0;
			//if (node.get_port_count() > 2) {
				//result += padding + tabbing + i + "\n";
				//deeper = 1;
			//}
			//foreach (var connection in port.connections) {
				//result += graph_nodes(connection.node, depth + 1 + deeper, used, connection);
			//}
		//}
//
		//if (depth == 0) {
			//result = "Graphed " + used.Count() + " nodes:\n\n" + result;
		//}
//
		//return result;
	//}
//
	//public static string graph_expressions (Expression expression, int depth = 0, List<Expression> used = null) {
		//if (used == null)
			//used = [];
//
		//var tabbing = " ";
		//var result = "";
		//var padding = "";
		//foreach (var i in 0...depth) {
			//padding += tabbing;
		//}
//
		//if (expression == null)
			//return padding + "null\n";
//
		//result += padding + expression.to_string() + "\n";
		//used.Add(expression);
//
		//foreach (var child in expression.children) {
			//result += graph_expressions(child, depth + 1, used);
		//}
//
		//if (depth == 0) {
			//result = "Graphed " + used.Count() + " expressions:\n\n" + result;
		//}
//
		//return result;
	//}
//
	//public void is_node_new (Node node) {
		//return this.new_nodes.Contains(node);
	//}
}}