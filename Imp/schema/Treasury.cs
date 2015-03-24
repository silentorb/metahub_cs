using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imperative.schema
{
   public class Treasury
    {
       public Dictionary<string, int> jewels = new Dictionary<string, int>();

       public Treasury(Dictionary<string, int> jewels)
       {
           this.jewels = jewels;
       }
    }
}
