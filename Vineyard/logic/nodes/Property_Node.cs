using System.Diagnostics;

using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes
{
    public class Property_Node : Node
    {
        public Property property;

        public Property_Node(Property property)

            : base(Node_Type.property)
        {
            this.property = property;
        }

        public override string debug_string
        {
            get { return property.fullname; }
        }

        override public Signature get_signature()
        {
            return property.get_signature();
        }

        public override Node clone()
        {
            return new Property_Node(property);
        }

    }
}