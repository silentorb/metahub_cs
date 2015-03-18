using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser.rhymes
{
    class Single_Rhyme : Rhyme
    {
        public Whisper whisper;

        public Single_Rhyme(string name)
            : base(name)
        {

        }

        public Single_Rhyme(string name, Whisper whisper)
            : base(name)
        {
            this.whisper = whisper;
        }

        public override void initialize(global::parser.Pattern_Source pattern, Parser parser)
        {
            var id = pattern.patterns[0].text;
            whisper = parser.lexer.whispers[id];
        }

        public override Legend_Result match(Runestone stone)
        {
            var result = check(whisper, stone);
            if (result != null)
                return result;

            if (stone.current.whisper.has_attribute(Whisper.Attribute.optional))
                return match(stone.next());

            stone.tracker.add_entry(false, this, stone.current);
            return null;

        }

        override public IEnumerable<Rhyme> aggregate()
        {
            return new List<Rhyme>();
        }

        private Legend_Result check(Whisper wisp, Runestone stone)
        {
            if (wisp == stone.current.whisper)
            {
                stone.tracker.add_entry(true, this, stone.current);
                return new Legend_Result(new String_Legend(this, stone.current.text), stone.next());
            }

            if (wisp.GetType() == typeof(Whisper_Group))
            {
                var group = (Whisper_Group)wisp;
                foreach (var child in group.whispers)
                {
                    var result = check(child, stone);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }
    }
}
