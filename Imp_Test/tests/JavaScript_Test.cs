using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using imp_test.fixtures;
using metahub.render.targets.js;

namespace imp_test.tests
{
    [TestFixture]
    public class JavaScript_Test
    {
        [Test]
        public void test_simple()
        {
            var target = new Js_Target();
            var overlord = Imp_Fixture.create_overlord(target, "simple.imp");
            var output = target.generate();
            var goal = Utility.load_resource("js.simple.js");
            Assert.AreEqual(goal, output);
        }
    }
}
