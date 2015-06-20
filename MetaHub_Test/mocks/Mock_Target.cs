using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative;
using imperative.render.artisan;
using metahub.render;

namespace metahub_test.mocks
{
    public class Mock_Target : Common_Target2
    {
        public Mock_Target(Overlord overlord)
            : base(overlord)
        {

        }

        public override void run(Overlord_Configuration config1, string[] sources)
        {
            throw new NotImplementedException();
        }

        protected override Stroke render_platform_function_call(imperative.expressions.Platform_Function expression, imperative.expressions.Expression parent)
        {
            throw new NotImplementedException();
        }
    }
}
