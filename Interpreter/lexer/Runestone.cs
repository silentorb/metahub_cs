using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using runic.parser;
using runic.parser.rhymes;

namespace runic.lexer
{
    [DebuggerDisplay("Runestone {position} {current.text}")]
    public class Runestone
    {
        List<Rune> runes; 
        int position;
        public Tracker tracker;
        static Rune end_of_input = new Rune(
            new String_Whisper("end of line", "end of line"), "end of line", null, null);

        public Rune current
        {
            get
            {
                return position < runes.Count
                    ? runes[position]
                    : end_of_input;
            }
        }

        public bool is_at_end
        {
            get { return position == runes.Count; }
        }

        public Runestone(List<Rune> runes)
        {
            this.runes = runes;
            tracker = new Tracker();
            position = 0;
        }

        public Runestone(List<Rune> runes, Tracker tracker, int position)
        {
            this.runes = runes;
            this.tracker = tracker;
            if (position > runes.Count)
            {
                position = runes.Count;
            }
            else if (position > tracker.furthest)
            {
                tracker.furthest = position;
            }

            this.position = position;
        }

        public Runestone next()
        {
            return new Runestone(runes, tracker, position + 1);
        }

        public int get_position()
        {
            return position;
        }
    }
}
