using System;
using System.Collections.Generic;
using System.Linq;

namespace parser
{
public class Pattern {
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
      ? previous.messages.Union(messages).ToList()
      : messages;
      return new_position == null ? result : __test__(new_position, depth);
    }

		//if (match.Count == 0)
			//throw new Exception("Match cannot be successful and have a length of 0");

    return result;
  }

  protected virtual Result __test__ (Position position, int depth) {
    throw new Exception("__test__ is an abstract function");
  }

  string debug_info () {
    return "";
  }

  protected Failure failure (Position start, Position end, List<Result> children = null) {
    return new Failure(this, start, end, children);
  }

  protected Match success (Position position, int length, List<Result> children = null, List<Match> matches = null) {
    Match match = new Match(this, position, length, children, matches);
		match.end = position.move(length);
		return match;
  }

    public virtual string get_text()
    {
        return "";
    }

//  Position rewind (Match match, List<string> messages) {
//    messages.Add("rewind " + type + " " + name + " " + match.start.get_coordinate_string());
//    var previous = match.last_success;
//    if (previous == null)
//      return null;
//
//    return previous.pattern.rewind(previous, messages);
//  }

  public virtual Pattern_Source get_data (Match match) {
    return null;
  }
}
}