using metahub.engine.Constraint_Operator;
using metahub.engine.Context;
using metahub.code.nodes.INode;
using metahub.engine.Node;

namespace metahub.schema
{
/**
 * ...
 * @author Christopher W. Johnson
 */

 /*
	*  The purpose of Property_Port is to keep the pipeline related functionality separate from
	*  Property, since there are cases where Property is simply used for schema definition.
	*
	*/

 /*
class Property_Port implements IPort {
	Property property;

	public List<IPort> connections = new List<IPort>();

	public Property_Port(Property property) {
		this.property = property;
	}

  public void connect (IPort other) {
    this.connections.Add(other);
    other.connections.Add(this);
  }

	public Kind get_type () {
		return property.type;
	}

	public Object get_value (Context context) {
		return context.node.get_value(property.id);
	}

	public Object set_value (Object value, Context context) {
		context.node.set_value(property.id, value);
		return value;
	}

	public void output (Object value, Context context) {
    foreach (var other in connections) {
      value = other.set_value(value, context);
    }

		return value;
	}
}*/}