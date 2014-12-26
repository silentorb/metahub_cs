
using System;
using System.Collections.Generic;
using metahub.schema;

namespace metahub.logic.schema {
public class Railway {

	public Region root_region;
	public Dictionary<string, Region> regions = new Dictionary<string, Region>();
	public string target_name;

	public Railway(Hub hub, string target_name) {
		this.target_name = target_name;
		
		root_region = new Region(hub.schema.root_space, "/");
		initialize_root_functions();

		foreach (var space in hub.schema.root_space.children) {
			if (space.name == "metahub")
				continue;

			Region region = new Region(space, target_name);
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
	
	
	//public metahub.imperative.types.Node translate (metahub Node.meta.types.Node) {
		//switch(Node.type) {
			//case metahub.meta.types.Expression_Type.literal:
				//metahub.meta.types.Literal literal = Node;
				//return new metahub.imperative.types.Literal(literal.value);
//
			//case metahub.meta.types.Expression_Type.function_call:
				//metahub.meta.types.Function_Call func = Node;
				//return new metahub.imperative.types.Function_Call(func.name, [translate(func.input)]);
//
			//case metahub.meta.types.Expression_Type.path:
				//return convert_path(Node);
//
			//case metahub.meta.types.Expression_Type.block:
				//metahub.meta.types.Block array = Node;
				//return new metahub.imperative.types.Create_Array(array.children.map((e)=> translate(e)));
//
			//default:
				//throw new Exception("Cannot convert Node " + Node.type + ".");
		//}
	//}
//
	//public metahub.imperative.types.Node convert_path (metahub Node.meta.types.Reference_Path) {
		//var path = Node.children;
		//List<metahub.imperative.types.Node> result = new List<metahub.imperative.types.Node>();
		//metahub.meta.types.Property_Reference first = path[0];
		//Rail rail = first.property.get_abstract_rail();
		//foreach (var token in path) {
			//if (token.type == metahub.meta.types.Expression_Type.property) {
				//metahub.meta.types.Property_Reference property_token = token;
				//var tie = rail.all_ties[property_token.property.name];
				//if (tie == null)
					//throw new Exception("tie is null: " + property_token.property.fullname());
//
				//result.Add(new metahub.imperative.types.Property_Reference(tie));
				//rail = tie.other_rail;
			//}
			//else {
				//metahub.meta.types.Function_Call function_token = token;
				//result.Add(new metahub.imperative.types.Function_Call(function_token.name, [], true));
			//}
		//}
		//return new metahub.imperative.types.Reference_Path(result);
	//}
	
	public Rail resolve_rail_path (List<string> path) {
		var tokens = path.copy();
		var rail_name = tokens.pop();
		var region = root_region;
		foreach (var token in tokens) {
			if (!region.children.ContainsKey(token))
				throw new Exception("Region " + region.name + " does not have space: " + token + ".");
				
			region = region.children[token];
		}
		
		if (!region.rails.ContainsKey(rail_name))
			throw new Exception("Region " + region.name + " does not have a rail named " + rail_name + ".");
			
		return region.rails[rail_name];
	}
	
	void initialize_root_functions () {
		root_region.add_functions([
		
			new Function_Info("count", [
				new Function_Version({ type: Kind.list, null rail }, { Kind type.Int })
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
}