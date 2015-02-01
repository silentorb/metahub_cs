using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using metahub.logic.schema;

namespace metahub.logic.nodes
{

    [DebuggerDisplay("function_call2 ({name})")]
    public class Function_Call2 : Node
    {
        public string name;
        public bool is_operation;

        public Function_Call2(string name, IEnumerable<Node> inputs, bool is_operation = false)
            : base(Node_Type.function_call)
        {
            this.name = name;
            this.is_operation = is_operation;

            if (inputs == null)
                throw new Exception("Function input cannot be null");

            foreach (var input in inputs)
            {
                connect_input(input);
            }
        }

        public override Node clone()
        {
            return new Function_Call2(name, new List<Node>(), is_operation);
        }
    }
}