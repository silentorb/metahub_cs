using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using metahub.Properties;
using metahub.schema;

namespace metahub.parser
{

    public class Pattern_Source
    {
        public string type;
        public string action;
        public string name;
        public string text;
        public bool? backtrack;

        public int? min;
        public int? max;

        public Pattern_Source pattern;
        public Pattern_Source divider;
        public List<Pattern_Source> patterns;
    }

    public class Definition
    {
        public List<Pattern> patterns = new List<Pattern>();
        public Dictionary<string, Pattern> pattern_keys = new Dictionary<string, Pattern>();

        public void load(Dictionary<string, Pattern_Source> source)
        {
            // First create all of the patterns
            foreach (var key in source.Keys)
            {
                var pattern = create_pattern(source[key], true);
                pattern.name = key;
                pattern_keys[key] = pattern;
                patterns.Add(pattern);
            }

            // Then finishing loading each one so that references can be resolved.
            foreach (var key in source.Keys)
            {
                //      trace(key);
                initialize_pattern(source[key], pattern_keys[key], true);
            }
        }

        Pattern __create_pattern(Pattern_Source source)
        {
            switch (source.type)
            {
                case "reference":
                    if (!pattern_keys.ContainsKey(source.name))
                        throw new Exception("There is no pattern named: " + source.name);

                    if (source.action != null)
                        return new Wrapper(pattern_keys[source.name], source.action);
                    else
                        return pattern_keys[source.name];

                case "regex":
                    return new Regex_Pattern(source.text);

                case "and":
                    return new Group_And();

                case "or":
                    return new Group_Or();

                case "repetition":
                    return new Repetition();

                case "literal":
                    return new Literal(source.text);
            }

            throw new Exception("Invalid parser pattern type: " + source.type + ".");
        }

        public Pattern create_pattern(Pattern_Source source, bool root = false)
        {
            if (root && source.type == "reference")
                return new Wrapper(null, null);

            var pattern = __create_pattern(source);
            if (pattern.type == null)
                pattern.type = source.type ?? "literal";

            if (source.backtrack.HasValue)
                pattern.backtrack = source.backtrack.Value;

            return pattern;
        }

        public void initialize_pattern(Pattern_Source source, Pattern pattern, bool root = false)
        {
            if (root && source.type == "reference")
            {
                if (!pattern_keys.ContainsKey(source.name))
                    throw new Exception("There is no pattern named: " + source.name);

                var wrapper = (Wrapper)pattern;
                wrapper.pattern = pattern_keys[source.name];
                if (source.action != null)
                    wrapper.action = source.action;

                return;
            }
            if (source.type == "and" || source.type == "or")
            {
                var group = (Group)pattern;
                if (source.action != null)
                    group.action = source.action;

                foreach (var child in source.patterns)
                {
                    var child_pattern = create_pattern(child);
                    //        trace("  " + key);
                    if (child_pattern == null)
                    {
                        //          trace(child);
                        throw new Exception("Null child pattern!");
                    }
                    initialize_pattern(child, child_pattern);
                    group.patterns.Add(child_pattern);
                }
            }
            else if (source.type == "repetition")
            {
                var repetition = (Repetition)pattern;
                repetition.pattern = create_pattern(source.pattern);
                initialize_pattern(source.pattern, repetition.pattern);
                //      trace("  [pattern]");

                //      trace("repi", source);
                repetition.divider = create_pattern(source.divider);
                initialize_pattern(source.divider, repetition.divider);

                if (source.min.HasValue)
                    repetition.min = source.min.Value;

                if (source.max.HasValue)
                    repetition.max = source.max.Value;

                if (source.action != null)
                    repetition.action = source.action;
            }
        }

        public void load_parser_schema()
        {
            var data = System.Text.Encoding.Default.GetString(Resources.parser);
            load(JsonConvert.DeserializeObject<Dictionary<string, Pattern_Source>>(data));
        }

    }
}