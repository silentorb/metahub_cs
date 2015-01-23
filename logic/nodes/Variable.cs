using metahub.logic.schema;

namespace metahub.logic.nodes
{

    /**
     * ...
     * @author Christopher W. Johnson
     */
    public class Variable : Node
    {
        public string name;
        public Signature signature;

        public Variable(string name, Signature signature)
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