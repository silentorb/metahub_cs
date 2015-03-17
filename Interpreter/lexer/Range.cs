using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace runic.lexer
{
   public class Range
   {
       public Position start;
       public Position end;

       public int length
       {
           get { return start.index - end.index; }
       }

       public Range(Position start, Position end)
       {
           this.start = start;
           this.end = end;
       }
   }
}
