using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.jackolantern.code
{
    public static class Dungeon_Carver
    {

        public static void generate_code2(JackOLantern jack, Dungeon dungeon, Rail rail)
        {
            var overlord = jack.overlord;
            var statements = dungeon.get_block("class_definition");
            if (overlord.logician.needs_hub)
            {
                var hub_dungeon = overlord.realms["metahub"].dungeons["Hub"];
                dungeon.add_portal(new Portal("hub", Kind.reference, dungeon, hub_dungeon));
            }
            dungeon.generate_initialize(statements.scope);

            foreach (var tie in rail.all_ties.Values)
            {
                if (tie.type == Kind.list)
                {
                    List_Code.common_functions(tie, jack, statements.scope);
                }
                else
                {
                    if (tie.has_setter())
                        Dungeon_Carver.generate_setter(overlord.get_portal(tie));
                }
            }

            if (dungeon.inserts != null)
            {
                foreach (var path in dungeon.inserts.Keys)
                {
                    var lines = dungeon.inserts[path];
                    var tokens = path.Split('.');
                    var block_name = tokens[0];

                    assure_setter(block_name, dungeon, jack);
                    var block = dungeon.get_block(block_name);
                    if (tokens.Length > 1)
                    {
                        block.add_many(tokens[1], lines.Select(s => new Insert(s)));
                    }
                    else
                    {
                        block.add_many(lines.Select(s => new Insert(s)));
                    }
                }
            }

            if (dungeon.rail != null && dungeon.rail.needs_tick)
            {
                dungeon.spawn_imp("tick");
                dungeon.interfaces.Add(overlord.realms["metahub"].dungeons["Tick_Target"]);
            }
        }

        public static void assure_setter(string path, Dungeon dungeon, JackOLantern jack)
        {
            if (dungeon.has_block(path))
                return;

            Imp new_imp;
            Block new_block; 
            var imp = dungeon.summon_imp(path, true);
            var tokens = path.Split('_');
            var portal_name = tokens.Last();

            var portal = dungeon.all_portals[portal_name];

            if (imp == null || imp.portal != null)
            {

                new_block = dungeon.get_block(path);
                new_imp = generate_setter(portal);
                if (imp != null)
                {
                    new_block.add("pre", Imp.setter(portal, new Variable(new_imp.parameters[0].symbol),
                         new Parent_Class(), null));
                }
            }
            else
            {
                new_imp = imp.spawn_child(dungeon);
                new_block = dungeon.create_block(path, new_imp.scope, new_imp.expressions);
                new_block.divide("pre").add(Imp.setter(portal, new Variable(new_imp.parameters[0].symbol),
                         new Parent_Class(), null));
            }
            new_block.divide("post");
        }

        public static Imp generate_setter(Portal portal)
        {
            var dungeon = portal.dungeon;
            var imp = dungeon.spawn_imp("set_" + portal.name);
            var function_scope = imp.scope;
            var value = function_scope.create_symbol("value", portal.get_profession());
            imp.parameters.Add(new Parameter(value));

            Function_Definition result = new Function_Definition(imp);
            var block = imp.block = dungeon.create_block("set_" + portal.name, new Scope(function_scope), result.expressions);

            var pre = block.divide("pre");

            var mid = block.divide(null, new List<Expression> {
			    new Flow_Control(Flow_Control_Type.If, new Operation("==", new List<Expression> {
					new Portal_Expression(portal), new Variable(value)
                }),
				    new List<Expression>{
					    new Statement("return")
                    }),
			    new Assignment(new Portal_Expression(portal), "=", new Variable(value))
		    });

            if (portal.type == Kind.reference && portal.other_portal != null)
            {
                var origin = function_scope.create_symbol("origin", new Profession(Kind.reference));
                imp.parameters.Add(new Parameter(origin, new Null_Value()));

                if (portal.other_portal.type == Kind.reference)
                {
                    mid.add(
                        Imp.setter(portal.other_portal, new Self(portal.dungeon),
                        new Portal_Expression(portal), new Self(portal.dungeon))
                    );
                }
                else
                {
                    mid.add(new Flow_Control(Flow_Control_Type.If, new Operation("&&", new List<Expression>
                        {
                            new Operation("!=", new List<Expression>
                            {
                                new Variable(origin), new Variable(value)
                            }),
                            new Operation("!=", new List<Expression>
                            {
                                new Portal_Expression(portal), new Null_Value()
                            }),
                        }), new List<Expression> {
                            Imp.setter(portal.other_portal, new Self(portal.dungeon), 
                            new Portal_Expression(portal), new Self(portal.dungeon))
                        //new Portal_Expression(portal,
                        //new Function_Call("add_" + portal.other_portal.name, null,
                        //    new List<Expression> { new Self(portal.dungeon), new Self(portal.dungeon) }))
                    
                    }));
                }
            }

            var post = block.divide("post");

            //if (tie.has_set_post_hook)
            //{
            //    post.add(new Function_Call(tie.get_setter_post_name(), null,
            //        new List<Expression> {
            //        new Variable(value)
            //    }));
            //}

            return imp;
        }
    }
}
