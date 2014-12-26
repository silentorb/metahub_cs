package metahub.parser;

class Literal extends Pattern {
  string value;
  string text;

  public Literal(string text) {
    this.text = text;
  }

  override Result __test__ (Position start, int depth) {
    if (start.context.text.substr(start.get_offset(), text.Count()) == text)
      return success(start, text.Count());

    return failure(start, start);
  }

  override Object get_data (Match match) {
    return text;
  }
}