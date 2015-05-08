using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imperative.schema;
using imperative.summoner;
using imperative.expressions;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.jackolantern.code
{
    public class Dwarf_Clan
    {
        private JackOLantern jack;
        public Dungeon dungeon;
        public Trellis trellis;
        public Dictionary<Minion, Dwarf> dwarves = new Dictionary<Minion, Dwarf>();

        public Dwarf_Clan(JackOLantern jack, Dungeon dungeon, Trellis trellis = null)
        {
            this.jack = jack;
            this.dungeon = dungeon;
            this.trellis = trellis;
            dungeon.on_add_minion += dungeon_on_add_minion;
            if (trellis != null)
                trellis.on_add_property += trellis_on_add_property;
        }

        void trellis_on_add_property(Trellis trellis, Property property)
        {
            if (dungeon.has_portal(property.name))
                return;

            jack.create_portal_from_property(property, dungeon);
        }

        void dungeon_on_add_minion(Dungeon dungeon, Minion minion)
        {
            dwarves[minion] = new Dwarf(this, minion);
        }

        public void generate_code1()
        {
            var class_definition = dungeon.get_block("class_definition");

            foreach (var tie in trellis.all_properties.Values)
            {
                if (tie.signature.is_list)
                {
                    List_Code.common_functions_stub(tie, jack, class_definition.scope);
                }
                else
                {
//                    if (tie.has_setter())
//                        generate_setter_stub(jack.get_portal(tie));
                }
            }
        }

        public void generate_code2()
        {
            var overlord = jack.overlord;
            var statements = dungeon.get_block("class_definition");
            if (jack.logician.needs_hub && !dungeon.has_portal("hub"))
            {
                var hub_dungeon = overlord.root.children["metahub"].dungeons["Hub"];
                dungeon.add_portal(new Portal("hub", Kind.reference, dungeon, hub_dungeon));
            }
            generate_initialize(statements.scope);

            foreach (var tie in trellis.all_properties.Values)
            {
                var portal = jack.get_portal(tie);
                if (tie.signature.is_list)
                {
                    List_Code.common_functions(portal, jack, statements.scope);
                }
                else
                {
//                    if (tie.has_setter())
//                        generate_setter(jack.get_portal(tie));
                }
            }

            if (dungeon.inserts != null)
            {
                foreach (var path in dungeon.inserts.Keys)
                {
                    var lines = dungeon.inserts[path];
                    var tokens = path.Split('.');
                    var block_name = tokens[0];

                    assure_setter(block_name);
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

            if (trellis.needs_tick)
            {
                dungeon.spawn_minion("tick");
                dungeon.interfaces.Add(overlord.root.children["metahub"].dungeons["Tick_Target"]);
            }
        }

        public void assure_setter(string path)
        {
            if (dungeon.has_block(path))
                return;

            Minion new_minion;
            Accordian newAccordian;
            var minion = dungeon.summon_minion(path, true);
            var tokens = path.Split('_');
            var portal_name = tokens.Last();

            var portal = dungeon.all_portals[portal_name];

            if (minion == null || minion.portal != null)
            {
                new_minion = generate_setter(portal);
                newAccordian = new_minion.accordian;
                if (minion != null)
                {
                    newAccordian.add("pre", Minion.setter(portal, new Variable(new_minion.parameters[0].symbol),
                         new Parent_Class(), null));
                }
            }
            else
            {
                new_minion = minion.spawn_child(dungeon);
                newAccordian = dungeon.create_block(path, new_minion.scope, new_minion.expressions);
                newAccordian.divide("pre").add(Minion.setter(portal, new Variable(new_minion.parameters[0].symbol),
                         new Parent_Class(), null));
            }
            newAccordian.divide("post");
        }

        public Minion generate_setter_stub(Portal portal)
        {
            if (portal.setter != null)
                return portal.setter;

            var minion_name = JackOLantern.get_setter_name(portal);
            var minion = dungeon.spawn_minion(minion_name);
            portal.setter = minion;
            var function_scope = minion.scope;
            var value = function_scope.create_symbol("value", portal.get_target_profession());
            minion.parameters.Add(new Parameter(value));

            Function_Definition result = new Function_Definition(minion);
            var block =
                minion.accordian = dungeon.create_block(minion_name, new Scope(function_scope), result.expressions);

            var pre = block.divide("pre");

            var context = new Summoner_Context(minion);
            context.set_pattern("portal", new Portal_Expression(portal));
            context.set_pattern("value", new Variable(value));
            block.divide("mid", jack.summon_snippet_block("reference_setter", context));

            if (portal.type == Kind.reference && portal.other_portal != null)
            {
                var origin = minion.scope.create_symbol("origin", new Profession(Kind.reference));
                minion.parameters.Add(new Parameter(origin, new Null_Value()));
            }

            block.divide("post");
            return minion;
        }

        public Minion generate_setter(Portal portal)
        {
            //if (portal.setter != null)
            //    throw new Exception("Portal " + portal.fullname + " already has a setter.");

            var minion = generate_setter_stub(portal);

            var block = minion.accordian;

            if (portal.type == Kind.reference && portal.other_portal != null)
            {
                if (portal.other_portal.type == Kind.reference)
                {
                    block.add("mid",
                        Minion.setter(portal.other_portal, new Self(portal.dungeon),
                        new Portal_Expression(portal), new Self(portal.dungeon))
                    );
                }
                else
                {
                    var origin = minion.scope.find_or_exception("origin");
                    var value = minion.scope.find_or_exception("value");
                    var context = new Summoner_Context(minion);

                    context.set_pattern("portal", new Portal_Expression(portal));
                    context.set_pattern("other_portal", new Portal_Expression(portal.other_portal));

                    block.add("mid", jack.summon_snippet("set_other", context));
                }
            }

            return minion;
        }
        
        public Minion generate_initialize(Scope scope)
        {
            var expressions = new List<Expression>();
            var minion = dungeon.spawn_minion("initialize", new List<Parameter>(), expressions);
//            var block = dungeon.create_block("initialize", scope, expressions);
            var block = minion.accordian;
            block.divide("pre");
            block.divide("post");
            if (dungeon.parent != null)
            {
                block.add(Minion.call_initialize(dungeon, dungeon.parent, new Parent_Class()));
            }

            if (jack.logician.needs_hub && (dungeon.name != "Hub" || dungeon.realm.name != "metahub"))
            {
                var hub_dungeon = dungeon.overlord.root.children["metahub"].dungeons["Hub"];
                var symbol = minion.scope.create_symbol("hub", new Profession(Kind.reference, hub_dungeon));
                minion.parameters.Add(new Parameter(symbol));
                var hub_portal = dungeon.all_portals["hub"];
                block.add("pre", new Assignment(new Self(dungeon, new Portal_Expression(hub_portal)),
                   "=", new Variable(symbol)));

                if (trellis != null && trellis.needs_tick)
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

            return minion;
        }

    }
}
