package metahub.parser;

class Pattern {
  public string action;
  public string name;
  public string type;
  public bool backtrack = false;

  public Result test (Position position, int depth) {
    var result = __test__(position, depth);
    if (!result.success && backtrack) {
      var previous = position.context.last_success;
      List<string> messages = new List<string>();
      var new_position = previous.start.context.rewind(messages);
      previous.messages = previous.messages != null
      ? previous.messages.concat(messages)
      : messages;
      if (new_position == null)
        return result;

      return __test__(new_position, depth);
    }

		//if (match.Count() == 0)
			//throw new Exception("Match cannot be successful and have a length of 0");

    return result;
  }

  Result __test__ (Position position, int depth) {
    throw new Exception("__test__ is an abstract function");
  }

  string debug_info () {
    return "";
  }

  Failure failure (Position start, Position end, List<Result> children = null) {
    return new Failure(this, start, end, children);
  }

  Match success (Position position, int length, List<Result> children = null, List<Match> matches = null) {
    Match match = new Match(this, position, length, children, matches);
		match.end = position.move(length);
		return match;
  }


//  Position rewind (Match match, List<string> messages) {
//    messages.Add("rewind " + type + " " + name + " " + match.start.get_coordinate_string());
//    var previous = match.last_success;
//    if (previous == null)
//      return null;
//
//    return previous.pattern.rewind(previous, messages);
//  }

  public Object get_data (Match match) {
    return null;
  }
}