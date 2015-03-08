using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using imp_test.fixtures;
using metahub.render.targets;

namespace imp_test.tests
{
    [TestFixture]
    public class JavaScript_Test
    {
        [Test]
        public void test_simple()
        {
            var target = new JavaScript();
            var overlord = Imp_Fixture.create_overlord(target, "imp.simple.imp");
            var output = target.generate();
            var goal = Utility.load_resource("js.simple.js");
            Utility.diff(goal, output);
        }
    }
}
