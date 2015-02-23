﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.logic;
using metahub.logic.schema;
using metahub.schema;
using Namespace = metahub.imperative.types.Namespace;

namespace metahub.jackolantern.code
{
    public static class Dungeon_Carver
    {

        public static void generate_code1(JackOLantern jack, Dungeon dungeon, Rail rail)
        {
            var class_definition = dungeon.get_block("class_definition");

            foreach (var tie in rail.all_ties.Values)
            {
                if (tie.type == Kind.list)
                {
                    List_Code.common_functions_stub(tie, jack, class_definition.scope);
                }
                else
                {
                    if (tie.has_setter())
                        Dungeon_Carver.generate_setter_stub(jack.get_portal(tie), jack);
                }
            }
        }

        public static void generate_code2(JackOLantern jack, Dungeon dungeon, Rail rail)
        {
            var overlord = jack.overlord;
            var statements = dungeon.get_block("class_definition");
            if (jack.logician.needs_hub && !dungeon.has_portal("hub"))
            {
                var hub_dungeon = overlord.realms["metahub"].dungeons["Hub"];
                dungeon.add_portal(new Portal("hub", Kind.reference, dungeon, hub_dungeon));
            }
            Dungeon_Carver.generate_initialize(dungeon, statements.scope, rail, jack);

            foreach (var tie in rail.all_ties.Values)
            {
                var portal = jack.get_portal(tie);
                if (tie.type == Kind.list)
                {
                    List_Code.common_functions(portal, jack, statements.scope);
                }
                else
                {
                    if (tie.has_setter())
                        Dungeon_Carver.generate_setter(jack.get_portal(tie), jack);
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

            if (rail.needs_tick)
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
                new_imp = generate_setter(portal, jack);
                new_block = new_imp.block;
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

        public static Imp generate_setter_stub(Portal portal, JackOLantern jack)
        {
            if (portal.setter != null)
                return portal.setter;

            var dungeon = portal.dungeon;
            var imp_name = JackOLantern.get_setter_name(portal);
            var imp = dungeon.spawn_imp(imp_name);
            portal.setter = imp;
            var function_scope = imp.scope;
            var value = function_scope.create_symbol("value", portal.get_profession());
            imp.parameters.Add(new Parameter(value));

            Function_Definition result = new Function_Definition(imp);
            var block =
                imp.block = dungeon.create_block(imp_name, new Scope(function_scope), result.expressions);

            var pre = block.divide("pre");

            var context = new Summoner.Context(imp);
            context.set_pattern("portal", new Portal_Expression(portal));
            context.set_pattern("value", new Variable(value));
            block.divide("mid", jack.summon_snippet_block("reference_setter", context));

            if (portal.type == Kind.reference && portal.other_portal != null)
            {
                var origin = imp.scope.create_symbol("origin", new Profession(Kind.reference));
                imp.parameters.Add(new Parameter(origin, new Null_Value()));
            }

            block.divide("post");
            return imp;
        }

        public static Imp generate_setter(Portal portal, JackOLantern jack)
        {
            //if (portal.setter != null)
            //    throw new Exception("Portal " + portal.fullname + " already has a setter.");

            var imp = generate_setter_stub(portal, jack);

            var block = imp.block;

            if (portal.type == Kind.reference && portal.other_portal != null)
            {
                if (portal.other_portal.type == Kind.reference)
                {
                    block.add("mid",
                        Imp.setter(portal.other_portal, new Self(portal.dungeon),
                        new Portal_Expression(portal), new Self(portal.dungeon))
                    );
                }
                else
                {
                    var origin = imp.scope.find_or_exception("origin");
                    var value = imp.scope.find_or_exception("value");
                    var context = new Summoner.Context(imp);

                    context.set_pattern("portal", new Portal_Expression(portal));
                    context.set_pattern("other_portal", new Portal_Expression(portal.other_portal));

                    block.add("mid", jack.summon_snippet("set_other", context));
                }
            }

            return imp;
        }


        public static Imp generate_initialize(Dungeon dungeon, Scope scope, Rail rail, JackOLantern jack)
        {
            var expressions = new List<Expression>();
            var block = dungeon.create_block("initialize", scope, expressions);
            block.divide("pre");
            block.divide("post");
            if (dungeon.parent != null)
            {
                block.add(Imp.call_initialize(dungeon, dungeon.parent, new Parent_Class()));
            }

            foreach (var portal in dungeon.all_portals.Values)
            {
                Portal_Carver.customize_initialize(jack, portal, block);
            }

            var imp = dungeon.spawn_imp("initialize", new List<Parameter>(), expressions);
            imp.block = block;

            if (jack.logician.needs_hub && (dungeon.name != "Hub" || dungeon.realm.name != "metahub"))
            {
                var hub_dungeon = dungeon.overlord.realms["metahub"].dungeons["Hub"];
                var symbol = imp.scope.create_symbol("hub", new Profession(Kind.reference, hub_dungeon));
                imp.parameters.Add(new Parameter(symbol));
                var hub_portal = dungeon.all_portals["hub"];
                block.add("pre", new Assignment(new Self(dungeon, new Portal_Expression(hub_portal)),
                   "=", new Variable(symbol)));

                if (rail != null && rail.needs_tick)
                {
                    var tick_targets = hub_dungeon.all_portals["tick_targets"];
                    block.add("pre", new Portal_Expression(hub_portal,
                        new Property_Function_Call(Property_Function_Type.set,
                            tick_targets, new List<Expression>
                                {
                                    new Self(dungeon)
                                })
                        ));
                }
            }

            return imp;
        }

    }
}
