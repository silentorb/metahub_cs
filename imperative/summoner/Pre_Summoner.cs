using System;
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
                        data.dividers = ((Repetition_Match) match).dividers.Select(d => d.get_data().patterns[1].text).ToArray();
                        break;

                    case "reference_token":
                        data.text = data.patterns[0].text;
                        break;

                    case "optional_assignment":
                        return data.patterns[3];

                    case "type_info":
                        return data.patterns[2];

                    case "optional_parameters":
                        return data.patterns[1];
                }

                return data;
            }
        }
    }
}
