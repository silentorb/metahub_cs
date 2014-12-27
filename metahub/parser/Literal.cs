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
      var offset = start.get_offset();
      if (offset + text.Length >= start.context.text.Length)
          return failure(start, start);

    if (start.context.text.Substring(offset, text.Length) == text)
      return success(start, text.Length);

    return failure(start, start);
  }

  override public object get_data(Match match)
  {
    return text;
  }
}
}