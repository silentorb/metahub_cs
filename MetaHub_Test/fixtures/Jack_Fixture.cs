using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative;
using imperative.summoner;
using metahub.jackolantern;
using metahub.jackolantern.schema;
using metahub.logic;
using metahub_test.mocks;
using vineyard_test.fixtures;

namespace metahub_test.fixtures
{
    static class Jack_Fixture
    {
        public static Swamp create_simple_swamp()
        {
            var logician = Vineyard_Fixture.load_script("test2");
            var jack = create_jack(logician);
            var context = new Summoner_Context();
            var swamp = new Swamp(jack, logician.functions[0], context);
            return swamp;
        }

        public static JackOLantern create_jack(Logician logician)
        {
            var overlord = new Overlord();
            var target = new Mock_Target(overlord);
            var jack = new JackOLantern(logician, overlord, target);
            jack.load_schema_from_vineyard();
            return jack;
        }
    }
}
