using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.expressions;
using metahub.logic.schema;
using metahub.schema;
using Expression = metahub.imperative.expressions.Expression;
using Function_Call = metahub.imperative.expressions.Class_Function_Call;
using Parameter = metahub.imperative.expressions.Parameter;
using Variable = metahub.imperative.expressions.Variable;

namespace metahub.jackolantern.code
{
    public class List_Code
    {
        public static void common_functions_stub(Tie tie, JackOLantern minion, Scope scope)
        {
            add_function_stub(tie, minion, scope);
            remove_function_stub(tie, minion, scope);
        }
        
        public static void add_function_stub(Tie tie, JackOLantern jack, Scope scope)
        {
            var rail = tie.rail;
            var portal = jack.get_portal(tie);
            var dungeon = jack.get_dungeon(tie.rail);

            var function_name = "add_" + tie.tie_name;
            var minion = dungeon.spawn_minion(function_name, null, new List<Expression>());
            portal.setter = minion;
            var signature = tie.get_other_signature();
            var profession = jack.get_profession(signature);
            var item = minion.add_parameter("item", profession).symbol;
            var origin = minion.add_parameter("origin", new Profession(Kind.reference), new Null_Value()).symbol;
            
            Function_Definition definition = new Function_Definition(minion);

            var block = dungeon.create_block(function_name, scope, definition.expressions);
            minion.accordian = block;
            var mid = block.divide("mid", new List<Expression> {
				new Platform_Function("add", new Portal_Expression(portal), new Expression[]{ new Variable(item) })
		});
            var post = block.divide("post");

        }
        
        public static void remove_function_stub(Tie tie, JackOLantern jack, Scope scope)
        {
        }

        public static void common_functions(Portal portal, JackOLantern minion, Scope scope)
        {
            add_function(portal, minion, scope);
            remove_function(portal, minion, scope);
        }

        public static void add_function(Portal portal, JackOLantern jack, Scope scope)
        {
            var minion = jack.get_setter(portal);
            var dungeon = portal.dungeon;
            var origin = minion.scope.find_or_exception("origin");
            var item = minion.scope.find_or_exception("item");
            var block = minion.accordian;
            if (portal.other_portal != null)
            {
                block.add("mid",
                    new Flow_Control(Flow_Control_Type.If, new Operation("!=", new List<Expression>
                {
                    new Variable(origin), new Variable(item)
                }), new List<Expression> {
                    jack.call_setter(portal.other_portal, new Variable(item), 
                    new Self(dungeon), new Self(dungeon))
                }));
            }
        }

        
        public static void remove_function(Portal portal, JackOLantern jack, Scope scope)
        {
            var dungeon = portal.dungeon;

            var function_name = "remove_" + portal.name;
            var function_scope = new Scope(scope);
            var item = function_scope.create_symbol("item", portal.get_target_profession());
            var origin = function_scope.create_symbol("origin", new Profession(Kind.reference));
            var minion = dungeon.spawn_minion(function_name, new List<Parameter>
                {
                    new Parameter(item),
                    new Parameter(origin, new Null_Value())
                }, new List<Expression>());
            Function_Definition definition = new Function_Definition(minion);

            var block = dungeon.create_block(function_name, scope, definition.expressions);
            var context = new Summoner.Context(minion);
            context.set_pattern("list", new Portal_Expression(portal));
            context.set_pattern("item", new Variable(item));
            var mid = block.divide(null, jack.summon_snippet_block("list_remove", context));
            //var mid = block.divide(null, new List<Expression>{
            //    new Flow_Control(Flow_Control_Type.If, new Platform_Function("contains",
            //        new Portal_Expression(portal),
            //        new List<Expression>
            //        {
            //          new Variable(item)  
            //        }), 
            //    new List<Expression> {
            //        new Statement("return")
            //     }),
            //     new Platform_Function("remove", new Portal_Expression(portal), new Expression[] {new Variable(item)})
            //});
            block.divide("post");

            if (portal.other_portal != null)
            {
                mid.add(Minion.call_remove(portal.other_portal, new Variable(item), new Self(dungeon)));
            }
        }

    }
}