namespace metahub.meta.types
{
    public class Literal_Value : Node
    {
        public object value;

        public Literal_Value(object value)
            : base(Node_Type.literal)
        {
            this.value = value;
        }

        public float get_float()
        {
            return value is int ? (int)value : (float)value;
        }
    }
}