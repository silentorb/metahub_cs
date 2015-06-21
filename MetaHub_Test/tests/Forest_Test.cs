using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.render.artisan;
using metahub.jackolantern;
using metahub.logic;
using metahub.main;
using metahub.render;
using metahub_test.fixtures;
using NUnit.Framework;

namespace metahub_test.tests
{
    [TestFixture]
    public class Forest_Test
    {
        [Test]
        public void js_test()
        {
            var overlord = Jack_Fixture.create_overlord("js", "forest.forest.imp");
            var target = new imperative.render.artisan.targets.JavaScript(overlord);
            var hub = new Hub();
            var logician = new Logician(hub.root);
            var jack = new JackOLantern(logician, overlord, target);
            Hub.dungeons_to_trellises(overlord.root, logician.schema, jack);

            foreach (var clan in jack.clans.Values)
            {
                foreach (var portal in clan.dungeon.all_portals.Values)
                {
                    clan.trellis.add_property(Hub.portal_to_property(portal, clan, jack));
                }
            }
            hub.parse_code(Utility.load_resource("forest.forest.mh"), logician);
            logician.analyze();
            jack.run();

            var strokes = target.generate_strokes();
            var passages = Painter.render_root(strokes).ToList();
            var segments = new List<Segment>();
            var output = Scribe.render(passages, segments);
            var goal = Utility.load_resource("forest.forest.js");
            Utility.diff(goal, output);

            //            var source_map = new Source_Map("imp.browser.js", new [] {"browser.imp"}, segments);
            //            var source_map_content = source_map.serialize();
            //            Utility.diff(Utility.load_resource("js.browser.js.map"), source_map_content);
        }
    }
}
