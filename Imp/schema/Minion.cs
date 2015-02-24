﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.expressions;

using metahub.schema;

namespace metahub.imperative.schema
{
    public class Minion
    {
        public static string[] platform_specific_functions = new string[]
            {
                "count",
                "add",
                "contains",
                "distance",
                "last",
                "pop",
                "remove",
                "rand",
                "setter"
            };

        public string name;
        public Dungeon dungeon;
        public Portal portal;
        public bool is_platform_specific;
        public List<Minion> invokers = new List<Minion>();
        public List<Minion> invokees = new List<Minion>();
        public List<Parameter> parameters;
        public List<Expression> expressions;
        public Profession return_type = new Profession(Kind.none);
        public Minion parent;
        public List<Minion> children = new List<Minion>();
        public Scope scope;
        public bool is_abstract = false;
        public Block block;

#if DEBUG
        public string stack_trace;
#endif

        public Minion(string name, Dungeon dungeon, Portal portal = null)
        {
            this.name = name;
            this.dungeon = dungeon;
            this.portal = portal;

#if DEBUG
            stack_trace = Environment.StackTrace;
#endif
        }

        //public Function_Call invoke(Minion invoker, IEnumerable<Expression> args = null)
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

        public Minion spawn_child(Dungeon new_dungeon)
        {
            var child = new_dungeon.spawn_minion(name, parameters, null, return_type, portal);
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

        public static Property_Function_Call setter(Portal portal, Expression value, Expression reference, Expression origin)
        {
            return new Property_Function_Call(Property_Function_Type.set, portal, origin != null
                ? new List<Expression> { value, origin }
                : new List<Expression> { value }
             ) { reference = reference };
        }

        public static Expression call_remove(Portal portal, Expression reference, Expression item)
        {
            return portal.type == Kind.reference
                ? setter(portal, new Null_Value(), null, null).set_reference(reference)
                : new Property_Function_Call(Property_Function_Type.remove, portal, new List<Expression>
                    {
                     item   
                    }) { reference = reference };
        }

        public static Expression call_initialize(Dungeon caller, Dungeon target, Expression reference)
        {
            var args = new List<Expression>();
            var minion = target.summon_minion("initialize");
            if (minion.parameters.Count > 0)
                args.Add(new Portal_Expression(caller.all_portals["hub"]));

            return new Class_Function_Call(minion, reference, args);
        }

        public Parameter add_parameter(string name, Profession profession, Expression default_value = null)
        {
            var symbol = scope.create_symbol(name, profession);
            var parameter = new Parameter(symbol, default_value);
            parameters.Add(parameter);
            return parameter;
        }

        public void add_to_block(Expression expression)
        {
            if (block == null)
                block = dungeon.create_block(name, scope, expressions);

            block.add(expression);
        }

        public void add_to_block(string division, Expression expression)
        {
            if (block == null)
                block = dungeon.create_block(name, scope, expressions);

            block.add(division, expression);
        }
    }
}