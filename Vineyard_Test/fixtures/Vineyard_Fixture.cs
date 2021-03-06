﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic;
using metahub.logic.nodes;
using metahub.schema;

namespace vineyard_test.fixtures
{
    public static class Vineyard_Fixture
    {
        public static Schema load_schema()
        {
            var json = Utility.load_resource("schema.json");
            var schema = new Schema("test");
            schema.load_from_string(json);
            return schema;
        }

        public static Logician load_script(string script_name)
        {
            var schema = Vineyard_Fixture.load_schema();
            var logician = new Logician(schema);
            var code = Utility.load_resource(script_name + ".mh");
            logician.apply_code(code);
            return logician;
        } 
    }
}
