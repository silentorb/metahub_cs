using metahub.logic.schema;

namespace metahub.logic.nodes
{

    public class Parameter_Node : Node
    {
        public string name;
        public Signature signature;

        public Parameter_Node(string name, Signature signature = null)
            : base(Node_Type.parameter)
        {
            this.name = name;
            this.signature = signature;
        }
    }
}