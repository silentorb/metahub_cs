using metahub.jackolantern.tools;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes
{
    public class Literal_Value : Node
    {
        public object value;
        public Kind kind;

        public Literal_Value(object value, Kind kind)
            : base(Node_Type.literal)
        {
            this.value = value;
            this.kind = kind;
        }

        public float get_float()
        {
            return value is int ? (int)value : (float)value;
        }

        public override schema.Signature get_signature()
        {
            return new Signature(kind);
        }

        public override Node clone(Transform transform)
        {
            return new Literal_Value(value, kind);
        }
    }
}