using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public delegate bool Node_Filter(Node node);

    [DebuggerDisplay("{debug_string}")]
    public class Node
    {
        public Node_Type type;
        public List<Node> connections = new List<Node>();
        public List<Node> inputs = new List<Node>();
        public List<Node> outputs = new List<Node>();

        public string stack_trace;

        public virtual string debug_string
        {
            get { return type + " node"; }
        }

        public Node(Node_Type type)
        {
            stack_trace = Environment.StackTrace;
            this.type = type;
        }

        virtual public Signature get_signature()
        {
            //throw new Exception(GetType().Name + " does not implement get_signature().");
            return new Signature(Kind.unknown);
        }

        public List<Node> ports(Dir dir)
        {
            return dir == Dir.In ? inputs : outputs;
        }

        public List<Node> ports(int dir)
        {
            return dir == 0 ? inputs : outputs;
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

        public Node get_last(Dir dir = Dir.In)
        {
            var current = this;
            while (current.ports(dir).Count > 0)
            {
                current = current.ports(dir)[0];
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

        public void replace_other(Node old_node, Node new_node)
        {
            if (!connections.Contains(old_node))
                throw new Exception("Cannot replace node because there is no existing connection.");

            if (connections.Contains(new_node))
                throw new Exception("Already connected to replacement node.");

            var index = inputs.IndexOf(old_node);
            if (index > -1)
            {
                disconnect(old_node);
                inputs.Insert(index, new_node);
                new_node.outputs.Add(this);
            }
            else
            {
                index = outputs.IndexOf(old_node);
                disconnect(old_node);
                outputs.Insert(index, new_node);
                new_node.inputs.Add(this);
            }

            connections.Add(new_node);
            new_node.connections.Add(this);
        }

        public void replace(Node new_node)
        {
            // Loop through both inputs and outputs
            for (int i = 0; i < 2; ++i)
            {
                var j = 1 - i;
                var port_list = ports(i).ToArray();
                foreach (var port in port_list)
                {
                    var index = port.ports(j).IndexOf(this);
                    disconnect(port);
                    port.ports(j).Insert(index, new_node);
                    new_node.ports(i).Add(port);
                    new_node.connections.Add(port);
                    port.connections.Add(new_node);
                }
            }
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

        public List<Node> aggregate(Dir dir, Node_Filter filter = null)
        {
            var result = new List<Node>();
            if (filter == null || filter(this))
            {
                result.Add(this);

                foreach (var node in ports(dir))
                {
                    result.AddRange(node.aggregate(dir, filter));
                }
            }
            return result;
        } 

    }
}