using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using imperative;
using imperative.expressions;
using metahub.jackolantern;
using metahub.logic;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;
using NUnit.Framework;

namespace metahub_test
{
    class Utility
    {
        public static string load_resource(string filename)
        {
            var path = "metahub_test.resources." + filename;
            var assembly = Assembly.GetExecutingAssembly();
            
            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                throw new Exception("Could not find file " + path + ".");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static Regex trim_lines = new Regex(@"[\t ]*\r?\n");
        public static void diff(string first, string second)
        {
            first = trim_lines.Replace(first, "\n");
            second = trim_lines.Replace(second, "\n");

            if (first == second)
                return;

            File.WriteAllText("first.txt", first);
            File.WriteAllText("second.txt", second);
            var cwd = Directory.GetCurrentDirectory();
            string arguments = "/x /s "
                + cwd + @"\first.txt "
                + cwd + @"\second.txt ";

            var a = @"C:\Program Files (x86)\WinMerge\WinMergeU.exe";
            var b = @"E:\Programs\WinMerge\WinMergeU.exe";
            var path = File.Exists(a)
                ? a
                : b;
            System.Diagnostics.Process.Start(path, arguments);
            Assert.AreEqual(first, second);
        }
    }
}
