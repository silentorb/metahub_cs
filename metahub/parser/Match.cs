using System;
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
    this.length = length;
    success = true;

    if (pattern.type == "regex" || pattern.type == "literal") {
      last_success = start.context.last_success;
      start.context.last_success = this;
    }

    this.children = children ?? new List<Result>();

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

  override public string debug_info ()
  {
      var a = start.get_offset();
      var b = length;
      if (a + b >= start.context.text.Length)
      {
          b = start.context.text.Length - a;
      }

    return start.context.text.Substring(a, b);
  }

  public Pattern_Source get_data () {
    var data = pattern.get_data(this);
      var result = (Pattern_Source)start.context.perform_action(pattern.action, data, this);
      if (result == null)
          throw new Exception();
      return result;
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