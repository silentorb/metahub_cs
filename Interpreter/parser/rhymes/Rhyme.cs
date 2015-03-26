using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        private bool initializing = false;
        protected List<Rhyme> _vertical_return_types;
        public List<Rhyme> vertical_return_types
        {
            get
            {
                if (_vertical_return_types == null)
                {
                    if (initializing)
                        return new List<Rhyme>();
//                        throw new Exception("Loop");
                    initializing = true;
                    _vertical_return_types = get_single_type();
                }
                return _vertical_return_types;
            }
        }

        protected Rhyme(Rhyme_Type type, string name)
        {
            this.name = name;
            this.type = type;
        }

        public abstract void initialize(Legend pattern, Parser parser);
        public abstract Legend_Result match(Runestone stone, Rhyme parent);
        public abstract IEnumerable<Rhyme> aggregate();

        protected abstract List<Rhyme> get_single_type();

        public static bool compare_lists(List<Rhyme> a, List<Rhyme> b)
        {
            if (a.Count != b.Count)
                return false;

            for (var i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        public bool returns(Rhyme rhyme)
        {
            return vertical_return_types.Contains(rhyme);
        }

        public virtual Rhyme type_rhyme
        {
            get { return this; }
        }
    }

}
