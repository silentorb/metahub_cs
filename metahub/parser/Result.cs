namespace metahub.parser
{
public class Result {
  public Position start;
  public List<Result> children;
  public bool success;
  public Pattern pattern;
  public List<string> messages;
	public Position end;

 public string debug_info () {
    return "";
  }
}
}