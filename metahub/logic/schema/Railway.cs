package metahub.logic.schema ;
using metahub.Hub;
using metahub.meta.types.Constraint;
using metahub.meta.types.Expression;
using metahub.meta.types.Expression_Type;
using metahub.meta.Scope;

using metahub.parser.Result;
using metahub.schema.Kind;
using metahub.schema.Trellis;

/**
 * ...
 * @author Christopher W. Johnson
 */

class Railway {

	public Region root_region;
	public Dictionary<string, Region> regions = new Dictionary<string, Region>();
	public string target_name;

	public Railway(Hub hub, string target_name) {
		this.target_name = target_name;
		
		root_region = new Region(hub.schema.root_namespace, "/");
		initialize_root_functions();

		foreach (var namespace in hub.schema.root_namespace.children) {
			if (space.name == "metahub")
				continue;

			Region region = new Region(namespace, target_name);
			regions[space.name] = region;
			root_region.children[region.name] = region;

			foreach (var trellis in space.trellises) {
				region.rails[trellis.name] = new Rail(trellis, this);
			}
		}

		foreach (var region in regions) {
			foreach (var rail in region.rails) {
				rail.process1();
			}
		}
		
		foreach (var region in regions) {
			foreach (var rail in region.rails) {
				rail.process2();
			}
		}
	}

	public static string get_class_name (expression) {
		return Type.getClassName(Type.getClass(expression)).split(".").pop();
	}

	public Rail get_rail (Trellis trellis) {
		return regions[trellis.space.name].rails[trellis.name];
	}
	
	
	//public metahub.imperative.types.Expression translate (metahub expression.meta.types.Expression) {
		//switch(expression.type) {
			//case metahub.meta.types.Expression_Type.literal:
				//metahub.meta.types.Literal literal = cast expression;
				//return new metahub.imperative.types.Literal(literal.value);
//
			//case metahub.meta.types.Expression_Type.function_call:
				//metahub.meta.types.Function_Call func = cast expression;
				//return new metahub.imperative.types.Function_Call(func.name, [translate(func.input)]);
//
			//case metahub.meta.types.Expression_Type.path:
				//return convert_path(cast expression);
//
			//case metahub.meta.types.Expression_Type.block:
				//metahub.meta.types.Block array = cast expression;
				//return new metahub.imperative.types.Create_Array(array.children.map((e)=> translate(e)));
//
			//default:
				//throw new Exception("Cannot convert expression " + expression.type + ".");
		//}
	//}
//
	//public metahub.imperative.types.Expression convert_path (metahub expression.meta.types.Path) {
		//var path = expression.children;
		//List<metahub.imperative.types.Expression> result = new List<metahub.imperative.types.Expression>();
		//metahub.meta.types.Property_Expression first = cast path[0];
		//Rail rail = cast first.property.get_abstract_rail();
		//foreach (var token in path) {
			//if (token.type == metahub.meta.types.Expression_Type.property) {
				//metahub.meta.types.Property_Expression property_token = cast token;
				//var tie = rail.all_ties[cast property_token.property.name];
				//if (tie == null)
					//throw new Exception("tie is null: " + property_token.property.fullname());
//
				//result.Add(new metahub.imperative.types.Property_Expression(tie));
				//rail = tie.other_rail;
			//}
			//else {
				//metahub.meta.types.Function_Call function_token = cast token;
				//result.Add(new metahub.imperative.types.Function_Call(function_token.name, [], true));
			//}
		//}
		//return new metahub.imperative.types.Path(result);
	//}
	
	public Rail resolve_rail_path (List<string> path) {
		var tokens = path.copy();
		var rail_name = tokens.pop();
		var region = root_region;
		foreach (var token in tokens) {
			if (!region.children.ContainsKey(token))
				throw new Exception("Region " + region.name + " does not have namespace: " + token + ".");
				
			region = region.children[token];
		}
		
		if (!region.rails.ContainsKey(rail_name))
			throw new Exception("Region " + region.name + " does not have a rail named " + rail_name + ".");
			
		return region.rails[rail_name];
	}
	
	void initialize_root_functions () {
		root_region.add_functions([
		
			new Function_Info("count", [
				new Function_Version({ type: Kind.list, null rail }, { Kind type.int })
			]),
			
			new Function_Info("cross", [
				new Function_Version({ type: Kind.list, null rail }, { Kind type.none })
			]),
			
			new Function_Info("dist", [
				new Function_Version({ type: Kind.list, null rail }, { Kind type.none })
			]),
		]);
	}
}