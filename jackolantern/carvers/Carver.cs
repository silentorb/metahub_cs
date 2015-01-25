using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.logic.nodes;
using metahub.logic.schema;

namespace metahub.jackolantern.pumpkins
{
    public abstract class Carver
    {
        protected JackOLantern jack;

        protected Carver(JackOLantern jack)
        {
            this.jack = jack;
        }

        public abstract void carve(Function_Call2 pumpkin);

        public List<Node> aggregate2(Node start)
        {
            var result = new List<Node> { start };

            foreach (var input in start.inputs)
            {
                result.AddRange(aggregate(input));
            }

            return result;
        }

        public List<Node> aggregate(Node start)
        {
            var result = new List<Node>();

            result.AddRange(start.inputs);
            foreach (var input in start.inputs)
            {
                result.AddRange(aggregate(input));
            }

            return result;
        }

        public IEnumerable<Portal> get_endpoints(Node start)
        {
            return aggregate(start)
                .OfType<Property_Reference>()
                .Select(p=>jack.overlord.get_portal(p.tie));
        }
    }

}
