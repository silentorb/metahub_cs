using System.Collections.Generic;

namespace metahub.parser
{
public class Group_Or : Group {

	public Group_Or() {

	}

  override protected Result __test__ (Position position, int depth) {
    List<Result> info_items = new List<Result>();
		var end = position;

    foreach (var pattern in patterns) {
      var result = pattern.test(position, depth + 1);
			if (result.end.get_offset() > end.get_offset())
				end = result.end;

      info_items.Add(result);
      if (result.success) {
//        match.children.Add(result);
//        result = info;
        var match = (Match)result;
        return success(position, match.length, info_items, new List<Match> { match });
      }
    }

    return failure(position, end, info_items);
  }

  override public object get_data(Match match)
  {
    return match.matches[0].get_data();
  }
}
}