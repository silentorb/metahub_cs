using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.schema;

namespace test.meta.fixtures
{
    public static class Hub_Fixture
    {
        public static Schema load_schema()
        {
            var json = Utility.load_resource("schema.json");
            var schema = new Schema("test");
            schema.load_from_string(json);
            return schema;
        }
    }
}
