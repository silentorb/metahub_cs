package metahub.parser;

class Wrapper extends Pattern {
  public Pattern pattern;

  public Wrapper(Pattern pattern, string action) {
    this.pattern = pattern;
    this.action = action;
  }

  override Result __test__ (Position start, int depth) {
    var result = pattern.test(start, depth);
    if (!result.success)
      return failure(start, start, [ result ]);

    Match match = cast result;

    return success(start, match.Count(), [ result ], [ match ]);
  }

  override Object get_data (Match match) {
    return match.matches[0].get_data();
  }
}