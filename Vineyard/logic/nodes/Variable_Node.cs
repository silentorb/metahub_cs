using metahub.logic.schema;

namespace metahub.logic.nodes
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
    public class Variable_Node : Node
    {
        public string name;
        public Signature signature;

        public Variable_Node(string name, Signature signature)
            : base(Node_Type.variable)
        {
            this.name = name;
            this.signature = signature;
        }

        public override Signature get_signature()
        {
            return signature;
        }
    }
}