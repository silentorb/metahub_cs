namespace metahub.parser
{
public class Bootstrap : Context {

//  public Bootstrap() { }

  public override object perform_action (string name, object data, Match match) {
    if (name == null)
      return data;

    switch(name) {

      case "group":
        return group(data);
      case "and_group":
        return and_group(data);
      case "or":
        return or_group(data);
      case "literal":
        return literal(data);
      case "pattern":
        return pattern(data, match);
      case "start":
        return start(data);
      case "repetition":
        return repetition(data);
      case "reference":
        return reference(data);
      case "regex":
        return regex(data);
      case "rule":
        return rule(data);

      default:
        throw new Exception("Invalid parser method: " + name + ".");
    }
  }

  object literal (object data) {
//    trace("data", data);
    return data[1];
  }

  object regex (object data) {
//    trace("data", data);
    return {
    type: "regex",
    text: data[1]
    };
  }

  object reference (object data) {
    return {
    type: "reference",
    name: data
    };
  }

  object and_group (object data) {
    return {
    type: "and",
    patterns: data
    };
  }

  object group (object data) {
//  trace("group", data);
    return data[2];
  }

  object or_group (object data) {
    return {
    type: "or",
    patterns: data
    };
  }

  object pattern (object data, Match match) {
//    trace("pattern:", data);

    List<object> value = data;
    var w = value.Count();
    if (data.Count() == 0)
      return null;
    else if (data.Count() == 1)
      return data[0];
    else
      return {
      type: "and",
      patterns: data
      };
  }

  object repetition (object data) {
//    trace("rule", data);
    var settings = data[1];
    var result = {
			type: "repetition",
			pattern: {
				type: "reference",
				name: settings[0]
			},
			divider: {
				type: "reference",
				name: settings[1]
			}
    };

    if (settings.Count() > 2) {
      Reflect.setField(result, "min", Std.int(settings[2]));
      if (settings.Count() > 3) {
        Reflect.setField(result, "max", Std.int(settings[3]));
      }
    }
    return result;
  }

  object rule (object data) {
//    trace("rule", data);
    List<object> value = data[4];
    return {
    name: data[0],
    value: value != null && value.Count() == 1 ? value[0] : value
    };
  }

  object start (object data) {
    object map < string > = {};
//    trace("start", data);

		List<object> items = data;
    foreach (var item in items) {
      Reflect.setField(map, item.name, item.value);
//      map.setField(item.name, item.value);
    }
//    trace("data", data);
    return map; //haxe.Json.parse(haxe.Json.stringify(map);
  }
}
}