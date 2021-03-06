using System.Collections.Generic;

namespace parser
{
    public class Failure : Result
    {
        public Failure(Pattern pattern, Position start, Position end, List<Result> children = null)
        {
            this.pattern = pattern;
            this.start = start;
            this.end = end;
            success = false;
            this.children = children ?? new List<Result>();
        }

    }
}