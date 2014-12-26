package metahub.parser;

class Regex extends Pattern {
  EReg regex;
  string text;

  public Regex(string text) {
    if (text.charAt(0) != "^")
      text = "^" + text;

    regex = new EReg(text, "");
    this.text = text;
  }

  override Result __test__ (Position start, int depth) {
    if (!regex.matchSub(start.context.text, start.get_offset())) {
//      trace(Position.pad(depth) + "regfail", text);
      return failure(start, start);
    }

    var match = regex.matched(0);
//    trace(Position.pad(depth) + "reg", text, match);
    return success(start, match.Count());
  }

  override Object get_data (Match match) {
    var start = match.start;
    regex.matchSub(start.context.text, start.get_offset());
    return regex.matched(0);
  }
}