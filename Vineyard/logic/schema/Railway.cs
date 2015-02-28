
using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.nodes;
using metahub.schema;
/*
namespace metahub.logic.schema {
public class Railway2 {

	public Namespace root;
	public Dictionary<string, Namespace> regions = new Dictionary<string, Namespace>();

	public Railway2(Hub hub, string target_name) {

//        root = new Namespace(hub.schema.root, "/");
//		initialize_root_functions();

        foreach (var space in hub.schema.root.children.Values)
        {
            //if (space.name == "metahub")
            //    continue;

//			Namespace region = new Namespace(space, target_name);
//			regions[space.name] = region;
//			root.children[region.name] = region;

			foreach (var trellis in space.trellises.Values) {
//				region.trellises[trellis.name] = new Trellis(trellis, this);
			}
		}

		foreach (var region in regions.Values) {
			foreach (var rail in region.trellises.Values) {
//				rail.process1();
			}
		}
		
		foreach (var region in regions.Values) {
			foreach (var rail in region.trellises.Values) {
//				rail.process2();
			}
		}
	}

    //public static string get_class_name (Node expression) {
    //    return Type.getClassName(Type.getClass(expression)).split(".").pop();
    //}

	public Trellis get_rail (Trellis trellis) {
		return regions[trellis.space.name].trellises[trellis.name];
	}

    //public Trellis get_rail(string name)
    //{
    //    foreach (var region in regions.Values)
    //    {
    //        if (region.trellises.ContainsKey(name))
    //            return region.trellises[name];
    //    }
        
    //    throw new Exception("Could not find rail " + name + ".");
    //}

	//public imperative.types.Node translate (metahub Node.meta.types.Node) {
		//switch(Node.type) {
			//case metahub.logic.types.Expression_Type.literal:
				//metahub.logic.types.Literal literal = Node;
				//return new imperative.types.Literal(literal.value);
//
			//case metahub.logic.types.Expression_Type.function_call:
				//metahub.logic.types.Function_Call func = Node;
				//return new imperative.types.Function_Call(func.name, [translate(func.input)]);
//
			//case metahub.logic.types.Expression_Type.path:
				//return convert_path(Node);
//
			//case metahub.logic.types.Expression_Type.block:
				//metahub.logic.types.Block array = Node;
				//return new imperative.types.Create_Array(array.children.map((e)=> translate(e)));
//
			//default:
				//throw new Exception("Cannot convert Node " + Node.type + ".");
		//}
	//}
//
	//public imperative.types.Node convert_path (metahub Node.meta.types.Reference_Path) {
		//var path = Node.children;
		//List<imperative.types.Node> result = new List<imperative.types.Node>();
		//metahub.logic.types.Property_Reference first = path[0];
		//Trellis rail = first.property.get_abstract_rail();
		//foreach (var token in path) {
			//if (token.type == metahub.logic.types.Expression_Type.property) {
				//metahub.logic.types.Property_Reference property_token = token;
				//var tie = rail.all_ties[property_token.property.name];
				//if (tie == null)
					//throw new Exception("tie is null: " + property_token.property.fullname());
//
				//result.Add(new imperative.types.Property_Reference(tie));
				//rail = tie.other_rail;
			//}
			//else {
				//metahub.logic.types.Function_Call function_token = token;
				//result.Add(new imperative.types.Function_Call(function_token.name, [], true));
			//}
		//}
		//return new imperative.types.Reference_Path(result);
	//}
	
	
}
}*/