﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using parser;
using runic.lexer;

namespace runic.parser
{
    public enum Rhyme_Type
    {
        and,
        or,
        repetition,
        single
    }

    [DebuggerDisplay("Rhyme {name}")]
    public abstract class Rhyme
    {
        public string name;
        public Rhyme_Type type;
        public virtual bool is_ghost { get { return false; } }

        public virtual string debug_info
        {
            get { return name; }
        }

        protected Rhyme(Rhyme_Type type, string name)
        {
            this.name = name;
            this.type = type;
        }

        public abstract void initialize(Pattern_Source pattern, Parser parser);
        public abstract Legend_Result match(Runestone stone, Rhyme parent);
        public abstract IEnumerable<Rhyme> aggregate();

        public abstract Rhyme get_single_type();
    }

}
