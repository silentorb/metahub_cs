using System;
using System.Text.RegularExpressions;

namespace metahub.parser
{
public class Regex_Pattern : Pattern {
  Regex regex;
  string text;

  public Regex_Pattern(string text)
  {
    if (text[0] != '^')
      text = "^" + text;

    regex = new Regex(text);
    this.text = text;
  }

  override protected Result __test__(Position start, int depth)
  {
      var match = regex.Match(start.context.text, start.get_offset());
    if (!match.Success) {
//      trace(Position.pad(depth) + "regfail", text);
      return failure(start, start);
    }

//    trace(Position.pad(depth) + "reg", text, match);
    return success(start, match.Length);
  }

  override public object get_data(Match match)
  {
    var start = match.start;
      //throw new Exception("Not implemented.");
      var result = regex.Match(start.context.text, start.get_offset());
      return result.Value;
      //return regex.matched(0);
  }
}
}