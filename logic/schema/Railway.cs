
using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.nodes;
using metahub.schema;

namespace metahub.logic.schema {
public class Railway {

	public Region root_region;
	public Dictionary<string, Region> regions = new Dictionary<string, Region>();

	public Railway(Hub hub, string target_name) {

        root_region = new Region(hub.schema.root_namespace, "/");
		initialize_root_functions();

        foreach (var space in hub.schema.root_namespace.children.Values)
        {
            //if (space.name == "metahub")
            //    continue;

			Region region = new Region(space, target_name);
			regions[space.name] = region;
			root_region.children[region.name] = region;

			foreach (var trellis in space.trellises.Values) {
				region.rails[trellis.name] = new Rail(trellis, this);
			}
		}

		foreach (var region in regions.Values) {
			foreach (var rail in region.rails.Values) {
				rail.process1();
			}
		}
		
		foreach (var region in regions.Values) {
			foreach (var rail in region.rails.Values) {
				rail.process2();
			}
		}
	}

    //public static string get_class_name (Node expression) {
    //    return Type.getClassName(Type.getClass(expression)).split(".").pop();
    //}

	public Rail get_rail (Trellis trellis) {
		return regions[trellis.space.name].rails[trellis.name];
	}

    //public Rail get_rail(string name)
    //{
    //    foreach (var region in regions.Values)
    //    {
    //        if (region.rails.ContainsKey(name))
    //            return region.rails[name];
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
		//Rail rail = first.property.get_abstract_rail();
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
	
	public Rail resolve_rail_path (IEnumerable<string> path) {
		var tokens = path.Take(path.Count() - 1);
		var rail_name = path.Last();
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
		root_region.add_functions(new List<Function_Info> {

			new Function_Info("contains", new List<Signature> {
				new Signature(Kind.Bool, new []
				    {
				        new Signature(Kind.list),
				        new Signature(Kind.reference)
				    })
			}),

			new Function_Info("count", new List<Signature> {
				new Signature(Kind.Int, new [] { new Signature(Kind.list)})
			}),
			
			new Function_Info("cross", new List<Signature> {
                new Signature(Kind.none, new []
                    {
                        new Signature(Kind.list),
                        new Signature(Kind.none, new []
                            {
                                new Signature(Kind.reference),
                                new Signature(Kind.reference)
                            }), 
                    })
			}),
			
			new Function_Info("distance", new List<Signature> {
                new Signature(Kind.Float, new []
                    {
                        new Signature(Kind.reference),
                        new Signature(Kind.Float, new []
                            {
                                new Signature(Kind.reference),
                                new Signature(Kind.reference),
                            }), 
                    })
			}),

            new Function_Info("first", new List<Signature> {
				new Signature(Kind.reference, new [] { new Signature(Kind.list)})
			}),

            new Function_Info("map", new List<Signature> {
                new Signature(Kind.none, new []
                    {
                        new Signature(Kind.list),
                        new Signature(Kind.none, new []
                            {
                                new Signature(Kind.reference),
                                new Signature(Kind.reference), 
                            }), 
                    })
			}),
		});
	}
}
}