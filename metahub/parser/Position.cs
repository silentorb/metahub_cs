namespace metahub.parser
{
public class Position {
  int offset;
  public int y;
  public int x;
  public Context context;

  public Position(Context context, int offset = 0, int y = 1, int x = 1) {
    this.context = context;
    this.offset = offset;
    this.y = y;
    this.x = x;
  }

  public int get_offset () {
    return offset;
  }

  public string get_coordinate_string () {
    return y + ":" + x + (this.context.draw_offsets ? " " + offset : "");
  }

  public Position move (int modifier) {
    if (modifier == 0)
      return this;

    Position position = new Position(context, offset, y, x);

    int i = 0;
    if (modifier > 0) {
      do {
        if (context.text[offset + i] == '\n') {
          ++position.y;
          position.x = 1;
        }
        else {
          ++position.x;
        }
      }
      while (++i < modifier && offset + i < context.text.Length);
    }
    position.offset += modifier;
    return position;
  }

  static public string pad (int depth) {
    var result = "";
    int i = 0;
    while (i++ < depth) {
      result += "  ";
    }
    return result;
  }
}

}