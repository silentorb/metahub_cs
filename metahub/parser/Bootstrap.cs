package metahub.parser;

class Bootstrap extends Context {

//  public Bootstrap() { }

  public override Object perform_action (string name, Object data, Match match) {
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

  Object literal (Object data) {
//    trace("data", data);
    return data[1];
  }

  Object regex (Object data) {
//    trace("data", data);
    return {
    type: "regex",
    text: data[1]
    };
  }

  Object reference (Object data) {
    return {
    type: "reference",
    name: data
    };
  }

  Object and_group (Object data) {
    return {
    type: "and",
    patterns: data
    };
  }

  Object group (Object data) {
//  trace("group", data);
    return data[2];
  }

  Object or_group (Object data) {
    return {
    type: "or",
    patterns: data
    };
  }

  Object pattern (Object data, Match match) {
//    trace("pattern:", data);

    List<Object> value = cast data;
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

  Object repetition (Object data) {
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

  Object rule (Object data) {
//    trace("rule", data);
    List<Object> value = cast data[4];
    return {
    name: data[0],
    value: value != null && value.Count() == 1 ? value[0] : value
    };
  }

  Object start (Object data) {
    Object map < string > = cast {};
//    trace("start", data);

		List<Object> items = data;
    foreach (var item in items) {
      Reflect.setField(map, item.name, item.value);
//      map.setField(item.name, item.value);
    }
//    trace("data", data);
    return map; //haxe.Json.parse(haxe.Json.stringify(map);
  }
}