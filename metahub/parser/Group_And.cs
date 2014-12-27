using System.Collections.Generic;
using System.Linq;

namespace metahub.parser
{
    public class Group_And : Group
    {
        override protected Result __test__(Position start, int depth)
        {
            int length = 0;
            var position = start;
            var end = position;
            List<Result> info_items = new List<Result>();
            List<Match> matches = new List<Match>();

            foreach (var pattern in patterns)
            {
                //trace("and", position.get_coordinate_string());
                var result = pattern.test(position, depth + 1);
                if (result.end.get_offset() > end.get_offset())
                    end = result.end;

                info_items.Add(result);
                if (!result.success)
                    return failure(start, end, info_items);

                var match = (Match)result;
                matches.Add(match);
                position = position.move(match.length);

                length += match.length;
            }

            return success(start, length, info_items, matches);
        }

        override public object get_data(Match match)
        {
            return match.matches.Select(child => child.get_data()).ToList();
        }
    }
}