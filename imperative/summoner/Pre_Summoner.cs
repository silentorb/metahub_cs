﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.parser;

namespace metahub.imperative.summoner
{
    public partial class Summoner
    {
        private class Pre_Summoner : Parser_Context
        {
            public Pre_Summoner(Definition definition)
                : base(definition)
            {

            }

            public override object perform_action(string name, Pattern_Source data, Match match)
            {
                if (data.type == null)
                    data.type = match.pattern.name;

                if (match.pattern.name == null)
                    return data;

                switch (match.pattern.name)
                {
                    case "start":
                        return data.patterns[1];

                    case "optional_expression":
                        return data.patterns[1];

                    case "expression":
                        //data.dividers = ((Repetition_Match) match).dividers.Select(d => d.get_data().patterns[1].text).ToArray();
                        return process_expression(data, (Repetition_Match)match);
                    //break;

                    case "reference_token":
                        data.text = data.patterns[0].text;
                        break;

                    case "optional_assignment":
                        return data.patterns[3];

                    case "type_info":
                        return data.patterns[2];

                    case "optional_arguments":
                        return data.patterns[1];

                    case "closed_expression":
                        return data.patterns[2];

                    case "arguments":
                        data.patterns = data.patterns[2].patterns;
                        break;

                    case "optional_array":
                        return data.patterns[2];

                    case "short_block":
                        data.type = "block";
                        data.patterns = new[] { data.patterns[1] };
                        break;

                    case "long_block":
                        data.type = "block";
                        data.patterns = data.patterns[2].patterns;
                        break;

                    case "id_with_optional_array":
                        data.type = "id";
                        data.text = data.patterns[0].text;
                        data.patterns = data.patterns[1].patterns;
                        break;

                    //case "block":
                    //    return data.patterns[0];
                }

                return data;
            }

            static Pattern_Source process_expression(Pattern_Source data, Repetition_Match match)
            {
                if (data.patterns.Length < 2)
                    return data;

                var rep_match = match;
                var dividers = rep_match.dividers
                    .Select(d => d.matches.First(m => m.pattern.name != "trim").get_data().text).ToList();

                var patterns = data.patterns.ToList();
                MetaHub_Context.group_operators(new[] { "/", "*" }, patterns, dividers);
                MetaHub_Context.group_operators(new[] { "+", "-" }, patterns, dividers);
                data.patterns = patterns.ToArray();

                if (dividers.Count > 0)
                    data.text = dividers[0];

                return data;
            }
        }
    }
}