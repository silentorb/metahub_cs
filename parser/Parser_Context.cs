using System;
using System.Collections.Generic;
using System.Linq;

namespace metahub.parser
{
    public class Parser_Context
    {

        public string text;
        public bool debug = false;
        public bool draw_offsets = false;
        protected Definition definition;
        //  List<Match> history;
        public Match last_success;

        public Parser_Context(Definition definition)
        {
            this.definition = definition;
        }

        public Result parse(string text, Pattern start, bool silent = true)
        {
            this.text = text;
            //if (definition.patterns.Count == 0)
            //    throw new Exception("Unable to parse; definition does not have any patterns.");

            var result = start.test(new Position(this), 0);
            if (result.success)
            {
                var match = (metahub.parser.Match)result;
                var offset = match.start.move(match.length);
                if (offset.get_offset() < text.Length)
                {
                    result.success = false;
                    if (!silent)
                    {
                        throw new Exception("Could not find match at " + offset.get_coordinate_string()
                        + " [" + text.Substring(offset.get_offset()) + "]");
                    }
                }
            }

            return result;
        }

        virtual public object perform_action(string name, Pattern_Source data, Match match)
        {
            return null;
        }

        public Position rewind(List<string> messages)
        {
            var previous = last_success;
            if (previous == null)
            {
                messages.Add("Could not find previous text match.");
                return null;
            }
            var repetition = previous.get_repetition(messages);
            int i = 0;
            while (repetition == null)
            {
                previous = previous.last_success;
                if (previous == null)
                {
                    messages.Add("Could not find previous text match with repetition.");
                    return null;
                }
                repetition = previous.get_repetition(messages);
                if (i++ > 20)
                    throw new Exception("Infinite loop looking for previous repetition.");
            }

            var pattern = (Repetition)repetition.pattern;
            if (repetition.matches.Count > pattern.min)
            {
                messages.Add("rewinding " + pattern.name + " " + previous.start.get_coordinate_string());
                repetition.matches.RemoveAt(repetition.matches.Count - 1);
                repetition.children.RemoveAt(repetition.matches.Count - 1);
                return previous.start;
            }

            //    var previous = match.last_success;
            //    if (previous == null) {
            messages.Add("cannot rewind " + pattern.name + ", No other rewind options.");
            return null;
            //    }

            //    messages.Add("cannot rewind " + pattern.name + ", looking for earlier repetition.");
            //    return previous.pattern.rewind(previous, messages);
        }
    }
}