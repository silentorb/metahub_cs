using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.logic.nodes;

namespace metahub.jackolantern.schema
{
    public class Endpoint
    {
        public Property_Reference node;
        public Portal portal;

        public Endpoint(Property_Reference node, Portal portal)
        {
            this.node = node;
            this.portal = portal;
        }
    }
}
