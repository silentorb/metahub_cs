namespace metahub.parser
{
public class Group_And : Group {

	public Group_And() {

	}

  override Result __test__ (Position start, int depth) {
    int length = 0;
    var position = start, end = position;
    List<Result> info_items = new List<Result>();
    List<Match> matches = new List<Match>();

    foreach (var pattern in patterns) {
//trace("and", position.get_coordinate_string());
      var result = pattern.test(position, depth + 1);
			if (result.end.get_offset() > end.get_offset())
				end = result.end;

      info_items.Add(result);
      if (!result.success)
        return failure(start, end, info_items);

      Match match = result;
      matches.Add(match);
      position = position.move(match.Count);

      length += match.Count;
    }

    return success(start, length, info_items, matches);
  }

  override object get_data (Match match) {
    List<object> result = new List<object>();
    foreach (var child in match.matches) {
      result.Add(child.get_data());
    }
    return result;
  }
}
}