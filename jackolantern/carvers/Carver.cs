using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.schema;
using imperative.expressions;
using metahub.jackolantern.schema;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.schema;

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
                .Where(p=> !p.tie.trellis.is_value)
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

        protected static Operation get_conditions(Expression start)
        {
            return new Operation("==", new Literal(true), new Literal(true));
            var expression = start;
            var conditions = new List<Expression>();
            if (expression.next == null)
                throw new Exception("Child expression cannot be null.");

            do
            {
                expression = expression.clone();
                expression.get_end().parent = null;
                conditions.Insert(0, new Operation("!=", new[] { expression, new Null_Value() }));
            }
            while (expression.next != null);

            return new Operation("&&", conditions);
        }

        private static Node[] get_property_path(Node node, List<Node[]> result)
        {
            Node[] path = null;
            foreach (var input in node.inputs)
            {
                if (input.type == Node_Type.property)
                {
                    path = get_property_path(input, result).Concat(new[] {node}).ToArray();
                }
                else
                {
                    get_non_property_path(input, result);
                }
            }

            return path ?? new[] { node };
        }

        private static void get_non_property_path(Node node, List<Node[]> result)
        {
            foreach (var input in node.inputs)
            {
                if (input.type == Node_Type.property)
                {
                    result.Add(get_property_path(input, result));
                }
                else
                {
                    get_non_property_path(input, result);
                }
            }
        }

        protected Expression get_conditions(Node start)
        {
            var paths = new List<Node[]>();
            get_non_property_path(start, paths);

            var used_ties = new List<Property>();
            var conditions = new List<Operation>();

            foreach (var path in paths)
            {
                foreach (Property_Node node in path)
                {
                    if (!used_ties.Contains(node.tie) || node.tie.other_trellis == null || node.tie.is_value)
                        continue;

                    used_ties.Add(node.tie);
                    conditions.Insert(0, new Operation("!=", new[] { 
                        new Portal_Expression(jack.get_portal(node.tie), new Null_Value())
                    }));
                }
            }

            if (conditions.Count == 0)
                //throw new Exception("Not supported.");
                return new Literal(false);

            return conditions.Count > 1
                ? new Operation("&&", conditions)
                : conditions[0];
        }
    }

}
