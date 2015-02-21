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
    }
}