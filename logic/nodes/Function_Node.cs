using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using metahub.jackolantern.tools;
using metahub.logic.schema;

namespace metahub.logic.nodes
{

    [DebuggerDisplay("function_call2 ({name})")]
    public class Function_Node : Node
    {
        static List<string> circular_operators = new List<string>
        {
            "+=",
            "-=",
            "*=",
            "/="
        };

        public string name;
        public bool is_operation;
        public List<Function_Node> children = new List<Function_Node>();
        public bool is_circular = false;


        public Function_Node(string name, IEnumerable<Node> args, bool is_operation = false)
            : base(Node_Type.function_call)
        {
            this.name = name;
            this.is_operation = is_operation;

            if (args == null)
                throw new Exception("Function input cannot be null");

            foreach (var input in args)
            {
                connect_input(input);
            }

            if (circular_operators.Contains(name))
            {
                is_circular = true;
                var property_node = (Property_Node)inputs.First();
                property_node.tie.rail.needs_tick = true;
            }
        }

        public override Node clone()
        {
            return new Function_Node(name, new List<Node>(), is_operation);
        }
    }
}