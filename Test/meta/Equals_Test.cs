using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using metahub.imperative.summoner;
using metahub.jackolantern.schema;

namespace test.meta
{
    [TestFixture]
    public class Equals_Test
    {
        [Test]
        public void test_equals()
        {
            var jack = Utility.create_jack();
            var script = Utility.load_resource("test.meta.resources.test1.mh");
            jack.logician.analyze();
            jack.run();
            var context = new Summoner.Context();
//            var swamp = new Swamp(jack, pumpkin, context);
        }
    }
}
