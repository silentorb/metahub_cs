using haxe.Json;

namespace r {
struct Group_Source {
	string type,
	string action,
	List<Object> patterns
}

struct Pattern_Source {
	string type,
	bool backtrack
}

class Definition {
  public List<Pattern> patterns = new List<Pattern>();
  public Dictionary<string, Pattern> pattern_keys = new Dictionary<string, Pattern>();

  public Definition() {
  }

  public void load (Object source) {
// First create all of the patterns
    foreach (var key in Reflect.fields(source)) {
      var pattern = create_pattern(source[key], true);
      pattern.name = key;
      pattern_keys[key] = pattern;
      patterns.Add(pattern);
    }

// Then finishing loading each one so that references can be resolved.
    foreach (var key in Reflect.fields(source)) {
//      trace(key);
      initialize_pattern(source[key], pattern_keys[key], true);
    }
  }

  Pattern __create_pattern (Object source) {
    if (Std.is(source, string))
      return new Literal(source);

    switch (source.type) {
      case "reference":
        if (!pattern_keys.ContainsKey(source.name))
          throw new Exception("There is no pattern named: " + source.name);

        if (source.ContainsKey("action"))
          return new Wrapper(pattern_keys[source.name], source.action);
        else
          return pattern_keys[source.name];

      case "regex":
        return new Regex(source.text);

      case "and":
        return new Group_And();

      case "or":
        return new Group_Or();

      case "repetition":
        return new Repetition();
    }

    trace(source);
    throw new Exception("Invalid parser pattern type: " + source.type + ".");
  }

  public Pattern create_pattern (Pattern_Source source, root = false) {
		if (root && source.type == "reference")
			return new Wrapper(null, null);

    var pattern = __create_pattern(source);
    if (pattern.type == null)
      pattern.type = source.type != null ? source.type : "literal";

    if (source.ContainsKey("backtrack"))
      pattern.backtrack = source.backtrack;

    return pattern;
  }

  public void initialize_pattern (Object source, Pattern pattern, root = false) {
		if (root && source.type == "reference") {
			if (!pattern_keys.ContainsKey(source.name))
				throw new Exception("There is no pattern named: " + source.name);

			Wrapper wrapper = cast pattern;
			wrapper.pattern = pattern_keys[source.name];
			if (source.ContainsKey("action"))
				wrapper.action = source.action;

			return;
		}
    if (source.type == "and" || source.type == "or") {
			Group_Source group_source = cast source;
      Group group = cast pattern;
      if (group_source.ContainsKey("action"))
        group.action = group_source.action;

      foreach (var child in group_source.patterns) {
        var child_pattern = create_pattern(child);
//        trace("  " + key);
        if (child_pattern == null) {
//          trace(child);
          throw new Exception("Null child pattern!");
        }
        initialize_pattern(child, child_pattern);
        group.patterns.Add(child_pattern);
      }
    }
    else if (source.type == "repetition") {
      Repetition repetition = cast pattern;
      repetition.pattern = create_pattern(source.pattern);
      initialize_pattern(source.pattern, repetition.pattern);
//      trace("  [pattern]");

//      trace("repi", source);
      repetition.divider = create_pattern(source.divider);
      initialize_pattern(source.divider, repetition.divider);

      if (source.ContainsKey("min"))
        repetition.min = source.min;

      if (source.ContainsKey("max"))
        repetition.max = source.max;

      if (source.ContainsKey("action"))
        repetition.action = source.action;
    }
  }

  public void load_parser_schema () {
    //var data = Utility.load_json("metahub/parser.json");
		//var data = macro File.getContent("metahub/parser.json");
		var data = metahub.Macros.insert_file_as_string("inserts/parser.json");
    load(Json.parse(data));
  }

}}