using System;
using System.Collections.Generic;
using System.Linq;
using metahub.jackolantern.tools;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes
{
    public enum Dir
    {
        In,
        Out
    }

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

            connections.Add(other);
            other.connections.Add(this);

            inputs.Add(other);
            other.outputs.Add(this);
        }

        public void connect(Node other, Dir dir)
        {
            if (dir == Dir.In)
            {
                connect_input(other);
            }
            else
            {
                connect_output(other);
            }
        }

        public void connect_many_inputs(IEnumerable<Node> inputs)
        {
            foreach (var input in inputs)
            {
                connect_input(input);
            }
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

        public void disconnect(Node other)
        {
            if (!connections.Contains(other))
                return;

            connections.Remove(other);
            inputs.Remove(other);
            outputs.Remove(other);

            other.connections.Remove(this);
            other.inputs.Remove(this);
            other.outputs.Remove(this);
        }

        public void replace(Node first, Node second)
        {
            if (!connections.Contains(first))
                throw new Exception("Cannot replace node because there is no existing connection.");

            if (connections.Contains(second))
                throw new Exception("Already connected to replacement node.");

            var index = inputs.IndexOf(first);
            if (index > -1)
            {
                disconnect(first);
                inputs.Insert(index, second);
                second.outputs.Add(this);
            }
            else
            {
                index = outputs.IndexOf(first);
                disconnect(first);
                outputs.Insert(index, second);
                second.inputs.Add(this);
            }

            connections.Add(second);
            second.connections.Add(this);
        }

        public Node get_other_input(Node node)
        {
            if (inputs.Count != 2)
                throw new Exception("Not yet supported.");

            return inputs.First(n => n != node);
        }

        public Node get_other_output(Node node)
        {
            if (outputs.Count != 2)
                throw new Exception("Not yet supported.");

            return outputs.First(n => n != node);
        }

        public virtual Node clone()
        {
            throw new Exception("Not implemented.");
        }

    }
}