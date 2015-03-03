using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using vineyard.transform;
using vineyard_test.fixtures;

namespace vineyard_test.tests
{
    [TestFixture]
    public class Map_Test
    {
        [Test]
        public void test_map()
        {
            var logician = Vineyard_Fixture.load_script("map");
            var transform = new Transform(logician.functions[0]).initialize_map();

        }
    }
}
