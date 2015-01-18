using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic.schema;
using metahub.logic.types;

namespace metahub.logic
{
    public class Logician
    {
        public List<Constraint> constraints = new List<Constraint>();
        public Dictionary<string, Constraint_Group> groups = new Dictionary<string, Constraint_Group>();

        public Constraint create_constraint(Node[] first, Node[] second, string op, Lambda lambda, Scope scope)
        {
            var constraint = new Constraint(first, second, op, lambda, scope.caller);

            var tie = imperative.code.Parse.get_end_tie(constraint.first);
            if (tie != null)
                tie.constraints.Add(constraint);

            //if (!scope.is_map)
                constraints.Add(constraint);

            return constraint;
        }
/*
        public void process(Node expression, Scope scope)
        {
            switch (expression.type)
            {
                case Node_Type.scope:
                    scope_expression((Scope_Expression)expression, scope);
                    break;

                case Node_Type.block:
                    block_expression(((Block)expression).children, scope);
                    break;

                case Node_Type.constraint:
                    create_constraint((Constraint)expression, scope);
                    break;

                case Node_Type.function_scope:
                    function_scope((Function_Scope)expression, scope);
                    break;

                case Node_Type.path:
                    block_expression(((Reference_Path)expression).children, scope);
                    break;

                case Node_Type.property:
                case Node_Type.function_call:
                    break;

                default:
                    throw new Exception("Cannot process Node of type :" + expression.type + ".");
            }
        }

        void scope_expression(Scope_Expression expression, Scope scope)
        {
            //Scope new_scope = new Scope(scope.hub, Node.scope_definition, scope);
            foreach (var child in expression.children)
            {
                process(child, expression.scope);
            }
        }

        void block_expression(IEnumerable<Node> expressions, Scope scope)
        {
            foreach (var child in expressions)
            {
                process(child, scope);
            }
        }

        void function_scope(Function_Scope expression, Scope scope)
        {
            process(expression.expression, scope);
            foreach (var child in expression.lambda.expressions)
            {
                process(child, expression.lambda.scope);
            }
        }

        void create_constraint(Constraint expression, Scope scope)
        {
            var rail = scope.rail;
            var constraint = new Constraint(expression);
            var tie = imperative.code.Parse.get_end_tie(constraint.first);
            //trace("tie", tie.rail.name + "." + tie.name);
            tie.constraints.Add(constraint);
            constraints.Add(constraint);
        }
        */
    }
}
