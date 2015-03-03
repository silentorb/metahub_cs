using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative;
using metahub.render;

namespace imp_test.fixtures
{
   public static class Imp_Fixture
    {
       public static Overlord create_overlord(Target target, string script_name)
       {
           var code = Utility.load_resource(script_name);
           var overlord = new Overlord(target);

           overlord.summon(code);
           overlord.flatten();
           overlord.post_analyze();
           return overlord;
       }
    }
}
