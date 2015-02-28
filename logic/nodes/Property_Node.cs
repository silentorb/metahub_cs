using System.Diagnostics;
using metahub.jackolantern.tools;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes
{
    public class Property_Node : Node
    {
        public Property tie;

        public Property_Node(Property tie)

            : base(Node_Type.property)
        {
            this.tie = tie;
        }

        public override string debug_string
        {
            get { return tie.fullname; }
        }

        override public Signature get_signature()
        {
            return tie.get_signature();
        }

        public override Node clone()
        {
            return new Property_Node(tie);
        }

    }
}