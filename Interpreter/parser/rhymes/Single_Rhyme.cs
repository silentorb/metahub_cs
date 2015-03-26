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
            : base(Rhyme_Type.single, name)
        {
        }

        public Single_Rhyme(string name, Whisper whisper)
            : base(Rhyme_Type.single, name)
        {
            this.whisper = whisper;
        }

        public Single_Rhyme(Whisper whisper)
            : base(Rhyme_Type.single, whisper.name)
        {
            this.whisper = whisper;
        }

        public override bool is_ghost
        {
            get { return whisper.type == Whisper_Type.text; }
        }

        public override void initialize(Legend pattern, Parser parser)
        {
            var id = pattern.text;
            whisper = parser.lexer.whispers[id];
        }

        public override Legend_Result match(Runestone stone, Rhyme parent)
        {
            var result = check(whisper, stone);
            if (result != null)
                return result;

            if (stone.current.whisper.has_attribute(Whisper.Attribute.optional))
                return match(stone.next(), parent);

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
                var legend = whisper.has_attribute(Whisper.Attribute.optional)
                    ? null
                    : new String_Legend(this, stone.current.text);

                return new Legend_Result(legend, stone.next());
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

        protected override List<Rhyme> get_single_type()
        {
            return whisper.GetType() == typeof(Whisper_Group)
                ? new List<Rhyme> { this }
                : new List<Rhyme>();
        }
    }
}
