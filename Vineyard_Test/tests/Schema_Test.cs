using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using test.meta.fixtures;

namespace test.meta
{
    [TestFixture]
    public class Schema_Test
    {
        [Test]
        public void test_loading()
        {
            var schema = Hub_Fixture.load_schema();
            Assert.AreEqual(2, schema.trellises.Count);
        }
    }
}
