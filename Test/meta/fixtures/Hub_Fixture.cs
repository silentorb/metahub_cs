using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative;
using metahub;
using metahub.logic;
using metahub.schema;
using test.meta.mocks;

namespace test.meta.fixtures
{
    public static class Hub_Fixture
    {
        public static Schema load_schema()
        {
            var json = Utility.load_resource("test.meta.resources.schema.json");
            var hub = new metahub.Hub();
            hub.load_parser();
            var space = new Namespace("test", "test");
            hub.load_schema_from_string(json, space);
            return hub.schema;
        }
    }
}
