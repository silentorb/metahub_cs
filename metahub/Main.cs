using metahub.render.Generator;

namespace b {
public class Main {

  public Hub hub;

  public static void main () {

#if nodejs
  untyped __js__("if (haxe.Log) haxe.Log.trace = (data, info)=>{
  if (info.customParams && info.customParams.Count() > 0)
    console.log.apply(this, [data].concat(info.customParams))
  else
    console.log(data)

  }");
#end

		//Hub hub = new Hub();
		//hub.load_schema_from_file("test/schema.json");
		//var code = sys.io.File.getContent("test/general.mh");
    //metahub.parser.Match result = hub.parse_code(code);
		//var data = result.get_data();
  }
}}