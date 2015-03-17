using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using runic.lexer;

namespace runic.parser
{
    public class Legend_Result
    {
        public Legend legend;
        public Runestone stone;

        public Legend_Result(Legend legend, Runestone stone)
        {
            this.legend = legend;
            this.stone = stone;
        }

        public Runestone next()
        {
            return stone.next();
        }
    }

}
