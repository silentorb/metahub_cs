using System.Collections.Generic;

namespace metahub.render
{

    public delegate string String_Delegate();

    public static class String_Extensions
    {
        public static string Join(this object[] array, string seperator)
        {
            return string.Join(seperator, array);
        }

        public static string Join<T>(this IEnumerable<T> array, string seperator)
        {
            return string.Join(seperator, array);
        }
    }

    public class Renderer
    {

        int depth = 0;
        //string content = "";
        string indentation = "";

        public Renderer()
        {

        }

        public string line(string text)
        {
            return indentation + text + "\n";
        }

        public Renderer indent()
        {
            ++depth;
            indentation += "\t";
            return this;
        }

        public Renderer unindent()
        {
            --depth;
            indentation = indentation.Substring(0, indentation.Length - 1);
            return this;
        }

        //public void add (string text) {
        //content += text;
        //return this;
        //}

        public string newline(int amount = 1)
        {
            int i = 0;
            var result = "";
            while (i++ < amount)
            {
                result += "\n";
            }
            return result;
        }

        public void finish()
        {
            //content = "";
            depth = 0;
            indentation = "";
        }

        public string pad(string content)
        {
            return content == ""
            ? content
            : newline() + content;
        }

    }
}