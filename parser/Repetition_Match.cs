using System.Collections.Generic;

namespace metahub.parser
{
    public class Repetition_Match : Match
    {
        public List<Match> dividers;

        public Repetition_Match(Pattern pattern, Position start, int length = 0,
                      List<Result> children = null, List<Match> matches = null)
        :base(pattern, start,length,children,matches)
        { }
    }
}