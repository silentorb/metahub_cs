using System.Collections.Generic;

namespace parser
{
public class Wrapper : Pattern {
  public Pattern pattern;

  public Wrapper(Pattern pattern, string action) {
    this.pattern = pattern;
    this.action = action;
  }

  override protected Result __test__ (Position start, int depth) {
    var result = pattern.test(start, depth);
    if (!result.success)
      return failure(start, start, new List<Result> {result });

    var match = (Match)result;

    return success(start, match.length, new List<Result> { result }, new List<Match> { match });
  }

  override public Pattern_Source get_data(Match match)
  {
    return match.matches[0].get_data();
  }
}
}