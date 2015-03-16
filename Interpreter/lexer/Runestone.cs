using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.parser;

namespace runic.lexer
{
    public class Runestone
    {
        List<Rune> runes; 
        int position;

        public Rune current
        {
            get
            {
                return position <= runes.Count
                    ? runes[position]
                    : null;
            }
        }

        public Runestone(List<Rune> runes, int position = 0)
        {
            this.runes = runes;
            this.position = position;
        }

        public Rune next()
        {
            if (position < runes.Count)
                ++position;

            return current;
        }

        public Runestone clone()
        {
            return new Runestone(runes, position);
        }

        public Legend_Result success(Legend legend)
        {
            return new Legend_Result(legend, this);
        }
    }
}
