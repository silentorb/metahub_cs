using System.IO;
using System.Reflection;

namespace metahub
{
    public static class Utility
    {
        public static string get_string_resource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string result;

            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        public static void clear_folder(string url)
        {
            //var fs = Nodejs.fs;
            //void walk (string dir) {
            //    var children = Nodejs.fs.readdirSync(dir);
            //    foreach (var child in children) {
            //        var name = dir + "/" + child;
            //        //trace(name);
            //        var stat = fs.statSync(name);
            //        if (stat != null && stat.isDirectory()) {
            //            walk(name);
            //            Nodejs.fs.rmdirSync(name);
            //        }
            //        else {
            //            Nodejs.fs.unlinkSync(name);
            //        }
            //    }
            //}
            //walk(url);
        }

        public static void create_file(string url, string contents)
        {
            File.WriteAllText(url, contents);
        }
    }
}
/*
using haxe.Json;

namespace b {
#if nodejs
using js.Node in Nodejs;
#end
public class Utility {

  public static object load_json (string url) {
    string json;
#if html5
	throw new Exception("Not supported with html5.");
#elseif nodejs
  json = Nodejs.fs.readFileSync(url, { encoding: "ascii" } );
#else
    throw new Exception("load_json() not supported for this compilation target.");
		//json = sys.io.File.getContent(url);
#end

    return Json.parse(json);
  }

	public static void clear_folder (string url) {
#if nodejs
	var fs = Nodejs.fs;
	void walk (string dir) {
		var children = Nodejs.fs.readdirSync(dir);
		foreach (var child in children) {
			var name = dir + "/" + child;
			//trace(name);
			var stat = fs.statSync(name);
			if (stat != null && stat.isDirectory()) {
				walk(name);
				Nodejs.fs.rmdirSync(name);
			}
			else {
				Nodejs.fs.unlinkSync(name);
			}
		}
	}
	walk(url);
#else
	throw "Not supported.";
#end
	}

	public static void create_file (string url, string contents) {
#if nodejs
	Nodejs.fs.writeFileSync(url, contents );
#else
	throw "Not supported.";
#end
	}

	public static void create_folder (string url) {
#if nodejs
	if (!Nodejs.fs.existsSync(url))
		Nodejs.fs.mkdirSync(url);
#else
	throw "Not supported.";
#end
	}
}}*/