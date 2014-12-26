using System.Collections.Generic;

namespace metahub.parser
{
public class Repetition : Pattern {
  public int min; // min < 1 means this pattern is optional
  public int max; // max < 1 is infinite
  public Pattern pattern;
  public Pattern divider;

  public Repetition(int min = 1, int max = 0) {
    this.min = min;
    this.max = max;
  }

  override protected Result __test__(Position start, int depth)
  {
    var context = start.context;
    var position = start, end = position;
    int step = 0;
    List<Match> matches = new List<Match>();
    int last_divider_length = 0;
    int length = 0;
    List<Result> info_items = new List<Result>();
    List<Match> dividers = new List<Match>();

    do {
      var result = pattern.test(position, depth + 1);
			if (result.end.get_offset() > end.get_offset())
				end = result.end;

			info_items.Add(result);
      if (!result.success) {
          break;
      }
      Match match = result;
      position = match.start.move(match.Count);
//      match.Count += last_divider_length;
      length += match.Count + last_divider_length;
      matches.Add(match);

      ++step;

// Divider
      result = divider.test(position, depth + 1);
			if (result.end.get_offset() > end.get_offset())
				end = result.end;

      info_items.Add(result);
      if (!result.success)
        break;

      match = result;
      dividers.Add(match);
      last_divider_length = match.Count;
      position = position.move(match.Count);
    }
    while (max < 1 || step < max);

    if (step < min)
      return failure(start, end, info_items);

    Repetition_Match final = new Repetition_Match(this, start, length, info_items, matches);
		final.end = end;
    final.dividers = dividers;
    return final;
  }

//  override Position rewind (Match match, List<string> messages) {
//    if (match.matches.Count > min) {
//      var previous = match.matches.pop();
//      messages.Add("rewinding " + name + " " + previous.start.get_coordinate_string());
//      match.children.pop();
//      return previous.start;
//    }
//
//    var previous = match.last_success;
//    if (previous == null) {
//      messages.Add("cannot rewind " + name + ", No other rewind options.");
//      return null;
//    }
//
//    messages.Add("cannot rewind " + name + ", looking for earlier repetition.");
//    return previous.pattern.rewind(previous, messages);
//  }

  override protected object get_data(Match match)
  {
    List<object> result = new List<object>();
    foreach (var child in match.matches) {
      result.Add(child.get_data());
    }
    return result;
  }
}
}