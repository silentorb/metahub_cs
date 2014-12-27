using System;
using System.Collections.Generic;

namespace metahub.parser
{
    public class Bootstrap : Context
    {

        //  public Bootstrap() { }

        //public class Pattern_Source
        //{
        //    public string type;
        //    public string text;
        //    public string name;
        //    public Pattern_Source[] patterns;
        //    public Pattern_Source pattern;
        //    public Pattern_Source divider;
        //    public Pattern_Source value;
        //    public int? min;
        //    public int? max;
        //}

        public Bootstrap(Definition definition)
            : base(definition)
        {
        }


        public override object perform_action(string name, object data, Match match)
        {
            if (name == null)
                return data;

            switch (name)
            {

                case "group":
                    return @group((object[])data);
                case "and_group":
                    return and_group((object[])data);
                case "or":
                    return or_group(data);
                case "literal":
                    return literal((object[])data);
                case "pattern":
                    return pattern((object[])data, match);
                case "start":
                    return start((Pattern_Source[])data);
                case "repetition":
                    return repetition((object[])data);
                case "reference":
                    return reference(data);
                case "regex":
                    return regex((object[])data);
                case "rule":
                    return rule((object[])data);

                default:
                    throw new Exception("Invalid parser method: " + name + ".");
            }
        }

        static object literal(object[] data)
        {
            //    trace("data", data);
            return data[1];
        }

        static object regex(object[] data)
        {
            //    trace("data", data);
            return new Pattern_Source
            {
                type = "regex",
                text = (string)data[1]
            };
        }

        static object reference(object data)
        {
            return new Pattern_Source
            {
                type = "reference",
                name = (string)data
            };
        }

        static object and_group(object[] data)
        {
            return new Pattern_Source
            {
                type = "and",
                patterns = (Pattern_Source[])data
            };
        }

        static object group(object[] data)
        {
            //  trace("group", data);
            return data[2];
        }

        static object or_group(object data)
        {
            return new Pattern_Source
            {
                type = "or",
                patterns = (Pattern_Source[])data
            };
        }

        static object pattern(object[] data, Match match)
        {
            //    trace("pattern:", data);

            if (data.Length == 0)
                return null;
            else if (data.Length == 1)
                return data[0];
            else
                return new Pattern_Source
                {
                    type = "and",
                    patterns = (Pattern_Source[])data
                };
        }

        static object repetition(object[] data)
        {
            //    trace("rule", data);
            var settings = (string[])data[1];
            var result = new Pattern_Source
            {
                type = "repetition",
                pattern = new Pattern_Source
                {
                    type = "reference",
                    name = settings[0]
                },
                divider = new Pattern_Source
                {
                    type = "reference",
                    name = settings[1]
                }
            };

            if (settings.Length > 2)
            {
                result.min = int.Parse(settings[2]);
                if (settings.Length > 3)
                {
                    result.max = int.Parse(settings[3]);
                }
            }
            return result;
        }

        static object rule(object[] data)
        {
            //var value = (Pattern_Source)data[4];
            //Pattern_Source val = null;
            //if (value != null && value.Length == 1)
            //    val = value[0];
            //else
            //    val = value;
            
            return new Pattern_Source
            {
                name = (string)data[0],
                value = (Pattern_Source)data[4]
            };
        }

        object start(IEnumerable<Pattern_Source> data)
        {
            var map = new Dictionary<string, Pattern_Source>();

            foreach (var item in data)
            {
                map[item.name] = item.value;
            }
            return map; //haxe.Json.parse(haxe.Json.stringify(map);
        }
    }
}