using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.schema
{
    [DebuggerDisplay("Endpoint ({portal.fullname})")]
    public class Endpoint
    {
        public Property_Node node;
        public Portal portal;

        public Endpoint(Property_Node node, Portal portal)
        {
            this.node = node;
            this.portal = portal;
        }
    }
}
