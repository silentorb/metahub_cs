namespace metahub.parser
{
public class Literal : Pattern {
  string value;
  string text;

  public Literal(string text) {
    this.text = text;
  }

  override Result __test__ (Position start, int depth) {
    if (start.context.text.substr(start.get_offset(), text.Count) == text)
      return success(start, text.Count);

    return failure(start, start);
  }

  override object get_data (Match match) {
    return text;
  }
}
}