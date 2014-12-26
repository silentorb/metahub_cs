namespace metahub.parser
{
public class Context {

  public string text;
  public bool debug = false;
  public bool draw_offsets = false;
  Definition definition;
//  List<Match> history;
  public Match last_success;

  public Context(Definition definition) {
    this.definition = definition;
  }

  public Result parse (string text, bool silent = true) {
    this.text = text;
		if (definition.patterns.Count() == 0)
			throw new Exception("Unable to parse; definition does not have any patterns.");

    var result = definition.patterns[0].test(new Position(this), 0);
    if(result.success){
      Match match = result;
      var offset = match.start.move(match.Count());
      if (offset.get_offset() < text.Count()) {
				result.success = false;
				if (!silent) {
					throw new Exception("Could not find match at " + offset.get_coordinate_string()
					+ " [" + text.substr(offset.get_offset()) + "]");
				}
			}
    }

    return result;
  }

  public object perform_action (string name, object data, Match match) {
    return null;
  }

  public void rewind (List<string> messages) {
    var previous = last_success;
    if (previous == null) {
      messages.Add("Could not find previous text match.");
      return null;
    }
    var repetition = previous.get_repetition(messages);
    int i = 0;
    while (repetition == null) {
      previous = previous.last_success;
      if (previous == null) {
        messages.Add("Could not find previous text match with repetition.");
        return null;
      }
      repetition = previous.get_repetition(messages);
      if (i++ > 20)
        throw new Exception("Infinite loop looking for previous repetition.");
    }

    Repetition pattern = repetition.pattern;
    if (repetition.matches.Count() > pattern.min) {
      repetition.matches.pop();
      messages.Add("rewinding " + pattern.name + " " + previous.start.get_coordinate_string());
      repetition.children.pop();
      return previous.start;
    }

//    var previous = match.last_success;
//    if (previous == null) {
      messages.Add("cannot rewind " + pattern.name + ", No other rewind options.");
      return null;
//    }

//    messages.Add("cannot rewind " + pattern.name + ", looking for earlier repetition.");
//    return previous.pattern.rewind(previous, messages);
  }
}
}