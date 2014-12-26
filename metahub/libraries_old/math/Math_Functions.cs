using metahub.code.functions.Function;
using metahub.engine.Context;
using metahub.engine.General_Port;

namespace h {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Math_Functions extends Function
{

	override public void set_value (int index, Object value, Context context, General_Port source = null) {

	}

	override private Object forward (List<Object> args) {

		switch (func) {
			case 0:
				return Math.random() * 600;
		}

		throw new Exception("No Math Function with id " + func + ".");
	}

}}