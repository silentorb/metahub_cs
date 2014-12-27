using metahub.schema;

namespace metahub.logic.schema
{
    /**
     * @author Christopher W. Johnson
     */

    public class Signature
    {
        public Kind type;
        public Rail rail;
        public bool is_value;
        public int is_numeric;

        public Signature()
        {

        }

        public Signature(Kind type, Rail rail = null)
        {
            this.type = type;
            this.rail = rail;
        }
    }
}