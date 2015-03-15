using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using metahub.render;

namespace parser
{

    public class Debug_Info
    {

        public static string color(string text, ConsoleColor code)
        {
            return "~@" + (char)code + text + "~@" + (char)ConsoleColor.Gray;
        }

        public static string pad(int depth)
        {
            var result = "";
            for (var i = 0; i < depth; ++i)
                result += "  ";

            return result;
        }

        public static string render_info(Result info, int depth = 0, string prefix = "")
        {
            var additional = info.debug_info();

            var tab = pad(depth);
            var text = info.pattern.type
            + (info.pattern.name != null ? " " + info.pattern.name : "")
            + " " + info.start.get_coordinate_string();

            //  if (prefix)
            //    text += " " + info.success

            text = info.success ? color(text, ConsoleColor.White) : color(text, ConsoleColor.Red);

            text = tab + prefix + text;

            //if (additional != null)
            //text += " " + additional.replace(/\r?\n/g, color("\\n", ConsoleColor.Magenta))

            text += render_info_children(info, depth + 1);

            if (info.messages != null)
                text += "\n" + color(info.messages.Select(m => tab + "  " + m).join("\n"), ConsoleColor.Cyan);
            return text;
        }

        public static string render_info_children(Result info, int depth)
        {
            if (info.children.Count == 0)
                return "";

            return "\n" + info.children.Select(child =>
                {
                    var rep = info.pattern as Repetition;
                    var prefix = rep != null && child.pattern == rep.divider
                                     ? color("<", ConsoleColor.Magenta)
                                     : "";
                    return render_info(child, depth, prefix);
                })
              .join("\n");
        }

        public static void write_color_string(string text)
        {
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (c == '~' && text[i + 1] == '@')
                {
                    Console.ForegroundColor = (ConsoleColor) text[i + 2];
                    i += 2;
                }
                else
                {
                    Debug.Write(text[i]);
                }
            }
        }

        public static void output(Result result)
        {
            var text = render_info(result);
            write_color_string(text);
        }
    }


}
