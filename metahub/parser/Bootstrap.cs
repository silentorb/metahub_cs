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


        public override object perform_action(string name, Pattern_Source data, Match match)
        {
            if (name == null)
                return data;

            switch (name)
            {

                case "group":
                    return group(data);
                case "and_group":
                    return and_group(data);
                case "or":
                    return or_group(data);
                case "literal":
                    return literal(data);
                case "pattern":
                    return pattern(data, match);
                case "start":
                    return start(data);
                case "repetition":
                    return repetition(data);
                case "reference":
                    return reference(data);
                case "regex":
                    return regex(data);
                case "rule":
                    return rule(data);

                default:
                    throw new Exception("Invalid parser method: " + name + ".");
            }
        }

        static object literal(Pattern_Source data)
        {
            return new Pattern_Source {
                type = "literal",
                text = data.patterns[1].text 
            };
        }

        static object regex(Pattern_Source data)
        {
            return new Pattern_Source
            {
                type = "regex",
                text = data.patterns[1].text
            };
        }

        static Pattern_Source reference(Pattern_Source data)
        {
            return new Pattern_Source
            {
                type = "reference",
                name = data.text
            };
        }

        static Pattern_Source and_group(Pattern_Source data)
        {
            return new Pattern_Source
            {
                type = "and",
                patterns = data.patterns
            };
        }

        static object group(Pattern_Source data)
        {
            return data.patterns[2];
        }

        static object or_group(Pattern_Source data)
        {
            return new Pattern_Source
            {
                type = "or",
                patterns = data.patterns
            };
        }

        static object pattern(Pattern_Source data, Match match)
        {

            if (data.patterns.Length == 0)
                return data;
            else if (data.patterns.Length == 1)
                return data.patterns[0];
            else
                return new Pattern_Source
                    {
                        type = "and",
                        patterns = data.patterns
                    };
        }

        static object repetition(Pattern_Source data)
        {
            var settings = data.patterns[1].patterns;
            var result = new Pattern_Source
            {
                type = "repetition",
                pattern = new Pattern_Source
                {
                    type = "reference",
                    name = settings[0].text
                },
                divider = new Pattern_Source
                {
                    type = "reference",
                    name = settings[1].text
                }
            };

            if (settings.Length > 2)
            {
                result.min = int.Parse(settings[2].text);
                if (settings.Length > 3)
                {
                    result.max = int.Parse(settings[3].text);
                }
            }
            return result;
        }

        static Pattern_Source rule(Pattern_Source data)
        {
            return new Pattern_Source
            {
                name = data.patterns[0].text,
                value = data.patterns[4]
            };
        }

        static Pattern_Source start(Pattern_Source data)
        {
            var map = new Dictionary<string, Pattern_Source>();

            foreach (var item in data.patterns)
            {
                if (item.value == null)
                    throw new Exception();
                map[item.name] = item.value;
            }
            return new Pattern_Source {
               dictionary = map
            };
        }
    }
}