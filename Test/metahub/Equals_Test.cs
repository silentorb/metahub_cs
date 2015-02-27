using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using metahub.schema;

namespace test
{
    [TestFixture]
    public class Equals_Test
    {
        [Test]
        public void test_equals()
        {
            var json = Utility.load_resource("test.metahub.resources.schema.json");
            var hub = new metahub.Hub();
            hub.load_parser();
            var space = new Namespace("test", "test");
            hub.load_schema_from_string(json, space);
        }
    }
}
