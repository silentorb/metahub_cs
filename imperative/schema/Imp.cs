using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.imperative.schema
{
    public class Imp
    {
        public static string[] platform_specific_functions = new string[]
            {
                "count",
                "add",
                "contains",
                "dist",
                "last",
                "pop",
                "remove",
                "rand"
            };

        public string name;
        public Dungeon dungeon;
        public Portal portal;
        public bool is_platform_specific;
        public List<Imp> invokers = new List<Imp>();
        public List<Imp> invokees = new List<Imp>();
        public List<Parameter> parameters;
        public List<Expression> expressions;
        public Signature return_type = new Signature(Kind.none);
        public Imp parent;
        public List<Imp> children = new List<Imp>();
        public Scope scope;
        public bool is_abstract = false;
        public Block block;

#if DEBUG
        public string stack_trace;
#endif

        public Imp(string name, Dungeon dungeon, Portal portal = null)
        {
            this.name = name;
            this.dungeon = dungeon;
            this.portal = portal;

#if DEBUG
            stack_trace = Environment.StackTrace;
#endif
        }

        //public Function_Call invoke(Imp invoker, IEnumerable<Expression> args = null)
        //{
        //    var invocation = new Function_Call(this, null, args);

        //    if (invoker != null)
        //    {
        //        if (!invoker.invokees.Contains(this))
        //            invoker.invokees.Add(this);

        //        if (!invokers.Contains(invoker))
        //            invokers.Add(invoker);
        //    }

        //    return invocation;
        //}

        public Imp spawn_child(Dungeon new_dungeon)
        {
            var child = new_dungeon.spawn_imp(name, parameters, null, return_type, portal);
            child.parent = this;
            children.Add(child);
            return child;
        }

        public static Flow_Control If(Expression expression, List<Expression> children)
        {
            return new Flow_Control(Flow_Control_Type.If, expression, children);
        }

        public static Literal False()
        {
            return new Literal(false);
        }

        public static Literal True()
        {
            return new Literal(true);
        }

        public static Operation operation(string op, Expression first, Expression second)
        {
            return new Operation(op, new List<Expression> { first, second });
        }

        public static Property_Function_Call setter(Tie tie, Expression value, Expression reference, Expression origin)
        {
            return new Property_Function_Call(Property_Function_Type.set, tie, origin != null
                ? new List<Expression> { value, origin }
                : new List<Expression> { value }
             ) { reference = reference };
        }

        public static Property_Function_Call setter(Portal portal, Expression value, Expression reference, Expression origin)
        {
            return new Property_Function_Call(Property_Function_Type.set, portal, origin != null
                ? new List<Expression> { value, origin }
                : new List<Expression> { value }
             ) { reference = reference };
        }

        public static Expression call_remove(Tie tie, Expression reference, Expression item)
        {
            return tie.type == Kind.reference
                ? setter(tie, new Null_Value(), null, null).set_reference(reference)
                : new Property_Function_Call(Property_Function_Type.remove, tie, new List<Expression>
                    {
                     item   
                    }) { reference = reference };
        }

        public static Expression call_initialize(Dungeon caller, Dungeon target, Expression reference)
        {
            var args = new List<Expression>();
            var imp = target.summon_imp("initialize");
            if (imp.parameters.Count > 0)
                args.Add(new Portal_Expression(caller.all_portals["hub"]));

            return new Class_Function_Call(imp, reference, args);
        }

        public Parameter add_parameter(string name, Profession profession, Expression default_value = null)
        {
            var symbol = scope.create_symbol(name, profession);
            var parameter = new Parameter(symbol, default_value);
            parameters.Add(parameter);
            return parameter;
        }
    }
}
