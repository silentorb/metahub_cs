using System.Collections.Generic;
using System.Linq;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.schema;
using Expression = metahub.imperative.types.Expression;
using Function_Call = metahub.imperative.types.Class_Function_Call;
using Parameter = metahub.imperative.types.Parameter;
using Variable = metahub.imperative.types.Variable;

namespace metahub.jackolantern.code
{
    public class List_Code
    {
        public static void common_functions_stub(Tie tie, JackOLantern imp, Scope scope)
        {
            add_function_stub(tie, imp, scope);
            remove_function_stub(tie, imp, scope);
        }
        
        public static void add_function_stub(Tie tie, JackOLantern jack, Scope scope)
        {
            var rail = tie.rail;
            var portal = jack.get_portal(tie);
            var dungeon = jack.get_dungeon(tie.rail);

            var function_name = "add_" + tie.tie_name;
            var imp = dungeon.spawn_imp(function_name, null, new List<Expression>());
            portal.setter_imp = imp;
            var signature = tie.get_other_signature();
            var profession = jack.get_profession(signature);
            var item = imp.add_parameter("item", profession).symbol;
            var origin = imp.add_parameter("origin", new Profession(Kind.reference), new Null_Value()).symbol;
            
            Function_Definition definition = new Function_Definition(imp);

            var block = dungeon.create_block(function_name, scope, definition.expressions);
            imp.block = block;
            var mid = block.divide("mid", new List<Expression> {
				new Platform_Function("add", new Portal_Expression(portal), new Expression[]{ new Variable(item) })
		});
            var post = block.divide("post");

        }
        
        public static void remove_function_stub(Tie tie, JackOLantern jack, Scope scope)
        {
        }

        public static void common_functions(Portal portal, JackOLantern imp, Scope scope)
        {
            add_function(portal, imp, scope);
            remove_function(portal, imp, scope);
        }

        public static void add_function(Portal portal, JackOLantern jack, Scope scope)
        {
            var imp = jack.get_setter(portal);
            var dungeon = portal.dungeon;
            var origin = imp.scope.find_or_exception("origin");
            var item = imp.scope.find_or_exception("item");
            var block = imp.block;
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
            var imp = dungeon.spawn_imp(function_name, new List<Parameter>
                {
                    new Parameter(item),
                    new Parameter(origin, new Null_Value())
                }, new List<Expression>());
            Function_Definition definition = new Function_Definition(imp);

            var block = dungeon.create_block(function_name, scope, definition.expressions);
            var context = new Summoner.Context(imp);
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
                mid.add(Imp.call_remove(portal.other_portal, new Variable(item), new Self(dungeon)));
            }
        }

    }
}