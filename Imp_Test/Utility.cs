using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using imperative;
using imperative.expressions;
using metahub.jackolantern;
using metahub.render;

namespace imp_test
{
    class Utility
    {
        public static string load_resource(string filename)
        {
            var path = "imp_test.resources." + filename;
            var assembly = Assembly.GetExecutingAssembly();
            
            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                throw new Exception("Could not find file " + path + ".");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd().Replace("\r\n", "\n");
        }

        //public static JackOLantern create_jack()
        //{
        //    var json = Utility.load_resource("test.meta.resources.schema.json");
        //    var hub = new metahub.Hub();
        //    hub.load_parser();
        //    var space = new Namespace("test", "test");
        //    hub.load_schema_from_string(json, space);
        //    var overlord = new Overlord();
        //    //            var railway = new Railway(hub, "cpp");
        //    var target = new Mock_Target(overlord);
        //    var logician = new Logician(hub.schema);
        //    //            var jack = new JackOLantern(logician, overlord, railway, target);
        //    //            return jack;
        //    return null;
        //}
    }
}
