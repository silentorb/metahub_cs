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

        public static Overlord create_overlord(string target, string script_name)
        {
            return create_overlord(target, new[] { script_name });
        }

        public static Overlord create_overlord(string target_name, string[] script_names)
        {
            var overlord = new Overlord(target_name);
            var summoner = new Summoner2(overlord);
            summoner.summon_many(script_names.Select(s =>
                overlord.summon_legend(Utility.load_resource(s), s)
            ));
            overlord.flatten();
            overlord.post_analyze();
            return overlord;
        }

        public static Overlord create_overlord_with_path(string target_name, string script_path)
        {
            var overlord = new Overlord(target_name);
            overlord.summon_input(new[] { script_path });
            overlord.flatten();
            overlord.post_analyze();
            return overlord;
        }

    }
}
