using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imperative.schema
{
   public class Treasury :IDungeon
    {
       public Dictionary<string, int?> jewels = new Dictionary<string, int?>();
       public Realm realm;
       public string name;

       public Treasury(string name, Dictionary<string, int?> jewels, Realm realm)
       {
           this.name = name;
           this.jewels = jewels;
           this.realm = realm;
       }
    }
}
