using System.Collections.Generic;
using System.Linq;
using metahub.schema;

namespace metahub.logic.schema
{
    /**
     * @author Christopher W. Johnson
     */

    public class Signature
    {
        public Kind type;
        public Trellis rail;
        public bool is_value;
        public int is_numeric;
        public Signature[] parameters;

        public Signature()
        {

        }

        public Signature(Kind type, Trellis rail = null)
        {
            this.type = type;
            this.rail = rail;
        }

        public Signature(Kind type, Signature[] parameters)
        {
            this.type = type;
            this.parameters = parameters;
        }

        public Signature clone()
        {
            var result = new Signature(type, rail);
            if (parameters != null)
            {
                result.parameters = parameters.Select(p => p.clone()).ToArray();
            }

            return result;
        }
    }
}