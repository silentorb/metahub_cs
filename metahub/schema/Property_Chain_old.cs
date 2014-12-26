using metahub.code.nodes.INode;
using metahub.engine.List_Port;
using metahub.engine.Node;
using metahub.Hub;

namespace metahub.schema {
/**
 * @author Christopher W. Johnson
 */

struct Property_Chain List<Property>;
public class Property_Chain_Helper {
	public static Property_Chain flip (Property_Chain chain) {
		Property_Chain result = new Property_Chain();
		var i = chain.Count() - 1;
		while (i >= 0) {
			if (chain[i].other_property != null)
				result.Add(chain[i].other_property);

			--i;
		}
		return result;
	}

  public static Property_Chain from_string (List<string> path, Trellis trellis, int start_index = 0) {
    Property_Chain result = new Property_Chain();
    foreach (var x in start_index...path.Count()) {
      var property = trellis.get_property(path[x]);
      result.Add(property);
      trellis = property.other_trellis;
    }

    return result;
  }

	//public static void resolve_node (Property_Chain chain, Node node) {
		//foreach (var link in chain) {
			//var id = node.get_value(link.id);
			//node = node.hub.nodes[id];
		//}
//
		//return node;
	//}
/*
	public static void perform (Property_Chain chain, Node node, Hub hub, action, int start = 0) {
		foreach (var i in start...chain.Count()) {
			var link = chain[i];
			if (link.type == Kind.list) {
				throw new Exception("Property_Chain.perform is not implemented for lists.");
				//List_Port list_port = node.get_port(link.id);
				//var array = list_port.get_array();
				//foreach (var j in array) {
					//perform(chain, hub.get_node(j), hub, action, i + 1);
				//}
				//return;
			}
			else if (link.type == Kind.reference) {
				var id = node.get_value(link.id, null);
				node = hub.nodes[id];
			}
			else {
				throw new Exception("Not supported: " + link.name);
			}

		}

		action(node);
	}
*/
}

}