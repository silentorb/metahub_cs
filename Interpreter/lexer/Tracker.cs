using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using runic.parser;

namespace runic.lexer
{
    public class Tracker
    {
        [DebuggerDisplay("{debug_info}")]
        public class Entry
        {
            public Rhyme rhyme;
            public Rune rune;
            public bool success;

            public Entry(Rhyme rhyme, Rune rune, bool success)
            {
                this.rhyme = rhyme;
                this.rune = rune;
                this.success = success;
            }

            public string debug_info
            {
                get { return (success ? "* " : "! ") + rhyme.name + " " + rune.text; }
            }
        }

        public int furthest;
        public List<Entry> history = new List<Entry>();
        public void add_entry(bool success, Rhyme rhyme, Rune rune)
        {
            history.Add(new Entry(rhyme, rune, success));
        }

    }
}
