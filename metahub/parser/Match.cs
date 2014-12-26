using System.Collections.Generic;

namespace metahub.parser
{
public class Match : Result {
  public int length;
  public List<Match> matches;
  public Match last_success;
  public Match parent;

  public Match(Pattern pattern, Position start, int length = 0,
                      List<Result> children = null, List<Match> matches = null) {
    this.pattern = pattern;
    this.start = start;
    this.Count = length;
    success = true;

    if (pattern.type == "regex" || pattern.type == "literal") {
      last_success = start.context.last_success;
      start.context.last_success = this;
    }

    this.children = children != null
    ? children
    : new List<Result>();

    if (matches != null) {
      this.matches = matches;
      foreach (var match in matches) {
        match.parent = this;
      }
    }
    else {
      this.matches = new List<Match>();
    }
  }

  override public string debug_info () {
    return start.context.text.substr(start.get_offset(), length);
  }

  public object get_data () {
    var data = pattern.get_data(this);
    return start.context.perform_action(pattern.action, data, this);
  }


  public Match get_repetition (List<string> messages) {
    if (parent == null) {
      messages.Add("Parent of " + pattern.name + " is null.");
      return null;
    }
    if (parent.pattern.type == "repetition") {
      return parent;
    }

    messages.Add("Trying parent of " + pattern.name + ".");
    return parent.get_repetition(messages);
  }


}
}