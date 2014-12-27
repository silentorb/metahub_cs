namespace metahub.parser
{
public class Literal : Pattern {
  string value;
  string text;

  public Literal(string text) {
    this.text = text;
  }

  override protected Result __test__(Position start, int depth)
  {
    if (start.context.text.Substring(start.get_offset(), text.Length) == text)
      return success(start, text.Length);

    return failure(start, start);
  }

  override public object get_data(Match match)
  {
    return text;
  }
}
}