using System;
using System.Collections.Generic;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes
{
    public class Node
    {
        public Node_Type type;
        public List<Node> connections = new List<Node>();
        public List<Node> inputs = new List<Node>();
        public List<Node> outputs = new List<Node>();
        public string stack_trace;

        protected Node(Node_Type type)
        {
            stack_trace = Environment.StackTrace;
            this.type = type;
        }

        virtual public Signature get_signature()
        {
            //throw new Exception(GetType().Name + " does not implement get_signature().");
            return new Signature(Kind.unknown);
        }

        public void connect_input(Node other)
        {
            if (connections.Contains(other))
                return;

            //if (other.output != null)
            //    throw new Exception("Other node already has a connected output node.");

            connections.Add(other);
            other.connections.Add(this);

            inputs.Add(other);
            other.outputs.Add(this);
        }

        public void connect_output(Node other)
        {
            if (connections.Contains(other))
                return;

            if (other.connections.Contains(this))
                return;

            connections.Add(other);
            other.connections.Add(this);

            other.inputs.Add(this);
            outputs.Add(other);
        }

        public List<Node> get_path()
        {
            var result = new List<Node>();
            result.Add(this);
            var current = this;
            while (current.inputs.Count > 0)
            {
                current = current.inputs[0];
                result.Add(current);
            }

            return result;
        }

        public Node get_last()
        {
            var current = this;
            while (current.inputs.Count > 0)
            {
                current = current.inputs[0];
            }

            return current;
        }
    }
}