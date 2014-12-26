using haxe.macro.Expr;
using haxe.macro.Context;

namespace b {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Macros {

	macro static public Expr insert_file_as_string (string path) {
		var full_path = haxe.macro.Context.resolvePath(path);
    var now_str = sys.io.File.getContent(full_path);

    // var result = Context.makeExpr(now_str, Context.currentPos());
    // shorter writing:
    var result = macro $v{now_str}; // haxe 3.0: $v(now_str);

    // Example 3) how to debug expressions
    // trace(result); // have a look at the syntax tree haxe has created using the macro keyword
    // sometimes trace fails, throw seems to work always
    // tinkerbell even provides a "expr to haxe code" feature:
    // tink.macro.tools.Printer.print(result);
    return result;
  }
}}