using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace test
{
    class Utility
    {
        public static string load_resource(string filename)
        {
            var path = "vineyard_test.resources." + filename;
            var assembly = Assembly.GetExecutingAssembly();
            
            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                throw new Exception("Could not find file " + path + ".");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
