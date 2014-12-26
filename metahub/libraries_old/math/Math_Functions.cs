using metahub.code.functions.Function;
using metahub.engine.Context;
using metahub.engine.General_Port;

namespace h {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Math_Functions : Function
{

	override public void set_value (int index, object value, Context context, General_Port source = null) {

	}

	override private object forward (List<object> args) {

		switch (func) {
			case 0:
				return Math.random() * 600;
		}

		throw new Exception("No Math Function with id " + func + ".");
	}

}}