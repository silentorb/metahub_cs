using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.jackolantern.schema;
using metahub.logic.nodes;
using metahub.logic.schema;

namespace metahub.jackolantern.carvers
{
    public abstract class Carver
    {
        protected JackOLantern jack;

        protected Carver(JackOLantern jack)
        {
            this.jack = jack;
        }

        public abstract void carve(Function_Node pumpkin);

        //public List<Node> aggregate2(Node start)
        //{
        //    var result = new List<Node> { start };

        //    foreach (var input in start.inputs)
        //    {
        //        result.AddRange(aggregate(input));
        //    }

        //    return result;
        //}

        public List<Node> aggregate(Node start, bool include_self = false)
        {
            var result = new List<Node>();
            if (include_self)
                result.Add(start);

            result.AddRange(start.inputs);
            foreach (var input in start.inputs)
            {
                result.AddRange(aggregate(input));
            }

            return result;
        }

        public IEnumerable<Portal> get_endpoints(Node start, bool include_self = false)
        {
            return aggregate(start, include_self)
                .OfType<Property_Node>()
                .Select(p=>jack.get_portal(p.tie));
        }

        public IEnumerable<Endpoint> get_endpoints3(Node start, bool include_self = false)
        {
            return aggregate(start, include_self)
                .OfType<Property_Node>()
                .Where(p=> !p.tie.rail.trellis.is_value)
                .Select(p => new Endpoint(p, jack.get_portal(p.tie)));
        }

        public List<Node> aggregate2(Node start, bool include_self = false)
        {
            var result = new List<Node>();
            if (include_self)
                result.Add(start);

            result.AddRange(start.outputs);
            foreach (var output in start.outputs)
            {
                result.AddRange(aggregate(output));
            }

            return result;
        }

        public List<Endpoint> get_endpoints4(Node start, bool include_self = false)
        {
            return aggregate2(start, include_self)
                .OfType<Property_Node>()
                .Select(p => new Endpoint(p, jack.get_portal(p.tie))).ToList();
        }
    }

}
