namespace metahub.parser
{
public class Wrapper : Pattern {
  public Pattern pattern;

  public Wrapper(Pattern pattern, string action) {
    this.pattern = pattern;
    this.action = action;
  }

  override Result __test__ (Position start, int depth) {
    var result = pattern.test(start, depth);
    if (!result.success)
      return failure(start, start, [ result ]);

    Match match = result;

    return success(start, match.Count(), [ result ], [ match ]);
  }

  override object get_data (Match match) {
    return match.matches[0].get_data();
  }
}
}