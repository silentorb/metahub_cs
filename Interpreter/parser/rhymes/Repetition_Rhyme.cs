using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser.rhymes
{
    public class Repetition_Rhyme : Rhyme
    {
        public int min; // min < 1 means this pattern is optional
        public int max; // max < 1 is infinite
        public Rhyme rhyme;
        public Rhyme divider;
        private bool has_variable_dividers = false;

        public Repetition_Rhyme(string name)
            : base(Rhyme_Type.repetition, name)
        {

        }

        public Repetition_Rhyme(Rhyme rhyme, Rhyme divider, int min, int max)
            : base(Rhyme_Type.repetition, null)
        {
            this.rhyme = rhyme;
            this.divider = divider;
            this.min = min;
            this.max = max;
            has_variable_dividers = !divider.is_ghost;
        }

        public Repetition_Rhyme(Rhyme rhyme, int min, int max)
            : base(Rhyme_Type.repetition, null)
        {
            this.rhyme = rhyme;
            this.min = min;
            this.max = max;
        }

        public override void initialize(Legend pattern, Parser parser)
        {
            if (pattern.type != "repetition")
                pattern = pattern.children[0];

            var patterns = pattern.children[0].children;
            rhyme = parser.get_whisper_rhyme(patterns[0].text);
            if (patterns.Count == 3)
            {
                min = int.Parse(patterns[1].text);
                max = int.Parse(patterns[2].text);
            }
            else
            {
                divider = parser.get_whisper_rhyme(patterns[1].text);
                min = int.Parse(patterns[2].text);
                max = int.Parse(patterns[3].text);
                //                has_variable_dividers = divider.aggregate().OfType<Or_Rhyme>().Any();
                has_variable_dividers = !divider.is_ghost;
            }
        }

        public override Legend_Result match(Runestone stone, Rhyme parent)
        {
            var matches = new List<Legend>();
            var dividers = new List<Legend>();
            Legend last_divider = null;

            var final_stone = stone;
            var track_dividers = divider != null && has_variable_dividers;

            do
            {
                var main_result = rhyme.match(stone, this);
                if (main_result == null)
                    break;

                matches.Add(main_result.legend);
                stone = final_stone = main_result.stone;
                if (track_dividers && last_divider != null)
                    dividers.Add(last_divider);

                if (divider != null)
                {
                    var divider_result = divider.match(stone, this);

                    if (divider_result == null)
                        break;

                    last_divider = divider_result.legend;
                    stone = divider_result.stone;
                }
            }
            while (max == 0 || matches.Count < max);

            if (matches.Count < min)
                return null;

            // The equivalent of ? in a regex
            if (max == 1 && min == 0)
            {
                if (matches.Count == 1)
                    return new Legend_Result(matches[0], final_stone);

                return new Legend_Result(null, final_stone) { store_legend = true };
            }

            return new Legend_Result(new Group_Legend(this, matches, dividers), final_stone);

            //            return divider != null && has_variable_dividers
            //                ? match_tracking_dividers(stone)
            //                : match_not_tracking_dividers(stone);
        }
        //
        //        public Legend_Result match_tracking_dividers(Runestone stone)
        //        {
        //            var matches = new List<Legend>();
        //            var dividers = new List<Legend>();
        //            Legend last_divider = null;
        //
        //            var final_stone = stone;
        //
        //            do
        //            {
        //                var main_result = rhyme.match(stone, this);
        //                if (main_result == null)
        //                    break;
        //
        //                matches.Add(main_result.legend);
        //                stone = final_stone = main_result.stone;
        //                if (last_divider != null)
        //                    dividers.Add(last_divider);
        //
        //                var divider_result = divider.match(stone, this);
        //
        //                if (divider_result == null)
        //                    break;
        //
        //                last_divider = divider_result.legend;
        //                stone = divider_result.stone;
        //            }
        //            while (max == 0 || matches.Count < max);
        //
        //            if (matches.Count < min)
        //                return null;
        //
        ////            if (matches.Count == 0)
        ////                return new Legend_Result(null, stone);
        //            if (max == 1)
        //                return new Legend_Result(matches[0], final_stone);
        //
        //            return new Legend_Result(new Group_Legend(this, matches, dividers), final_stone);
        //        }

        //        public Legend_Result match_not_tracking_dividers(Runestone stone)
        //        {
        //            var matches = new List<Legend>();
        //            var final_stone = stone;
        //            do
        //            {
        //                var main_result = rhyme.match(stone, this);
        //                if (main_result == null)
        //                    break;
        //
        //                matches.Add(main_result.legend);
        //                stone = final_stone = main_result.stone;
        //
        //                if (divider != null)
        //                {
        //                    var divider_result = divider.match(stone, this);
        //                    if (divider_result == null)
        //                        break;
        //
        //                    stone = divider_result.stone;
        //                }
        //            }
        //            while (max == 0 || matches.Count < max);
        //
        //            if (matches.Count < min)
        //                return null;
        //
        ////            if (matches.Count == 0)
        ////                return new Legend_Result(null, stone);
        //            if (matches.Count == 1 && max == 1)
        //                return new Legend_Result(matches[0], final_stone);
        //
        //            return new Legend_Result(new Group_Legend(this, matches), final_stone);
        //        }

        override public IEnumerable<Rhyme> aggregate()
        {
            return divider != null
                ? new[] { rhyme, divider }
                : new[] { rhyme };
        }

        public override string debug_info
        {
            get { return "rep " + (name ?? rhyme.name); }
        }

        protected override List<Rhyme> get_single_type()
        {
            return rhyme.vertical_return_types;
        }

        public override Rhyme type_rhyme
        {
            get { return rhyme; }
        }
    }
}
