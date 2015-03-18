using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using runic.parser;

namespace runic.lexer
{
    [DebuggerDisplay("Runestone {position} {current.text}")]
    public class Runestone
    {
        List<Rune> runes; 
        int position;
        public Tracker tracker;

        public Rune current
        {
            get
            {
                return position <= runes.Count
                    ? runes[position]
                    : null;
            }
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
            if (position >= runes.Count)
            {
                position = runes.Count - 1;
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

//        public Runestone clone()
//        {
//            return new Runestone(runes, tracker, position);
//        }
    }
}
