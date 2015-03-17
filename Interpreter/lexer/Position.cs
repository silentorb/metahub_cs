using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
    public struct Position
    {
        public int x;
        public int y;
        public int index;

        public Position(int x, int y, int index)
        {

        }

        public static Position operator +(Position first, Position second)
        {
            return new Position();
        }
    }
}
