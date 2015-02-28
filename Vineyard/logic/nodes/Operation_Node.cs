using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.logic.nodes
{
    //public class Operation_Node : Node
    //{
    //    public string op;

    //    public Operation_Node(string op, IEnumerable<Node> children)
    //        : base(Node_Type.function_call)
    //    {
    //        this.op = op;
    //        connect_many_inputs(children);
    //        //var operations = inputs.OfType<Operation_Node>().ToArray();
    //        //foreach (var operation_node in operations)
    //        //{
    //        //    var operation = operation_node;
    //        //    var others = inputs.Where(c => c != operation);
    //        //    var inverse = Logician.inverse_operators[operation.op];
    //        //    foreach (var other in others)
    //        //    {
    //        //        var index = inputs.IndexOf(other);
    //        //        inputs.Remove(other);
    //        //        var new_operation = new Operation_Node(inverse, )
    //        //    }
    //        //}
    //    }

    //    public override Node clone()
    //    {
    //        return new Operation_Node(op, new Node[] {});
    //    }
    //}
}
