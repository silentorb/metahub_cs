using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.logic.nodes;

namespace metahub.jackolantern.pumpkins
{
    public class Contains : Carver
    {
        public Contains(JackOLantern jack)
            : base(jack)
        {

        }

        public override void carve(Function_Call2 pumpkin)
        {
            var list = pumpkin.inputs[0];
            var item = pumpkin.inputs[1];

            var endpoints = get_endpoints2(list);
            foreach (var endpoint in endpoints)
            {
                var dungeon = endpoint.dungeon;
                var initialize = jack.get_initialize(endpoint.dungeon);
                var context = new Summoner.Context(dungeon);
                context.scope = initialize.scope;
                context.add_pattern("list", () => jack.translate(list, context.scope));
                context.add_pattern("item", () => jack.translate(item, context.scope));
                initialize.block.add(jack.overlord.summon_snippet(jack.templates["contains"], context));
                break;
                initialize.block.add_many("post", new List<Expression>
                    {
                        Imp.If(new Operation("&&", new List<Expression>
                            {
                                new Operation("!=", new List<Expression>
                                    {
                                        jack.translate(item),
                                        new Null_Value()
                                    }),
                                new Operation("==", new List<Expression>
                                {
                                    new Platform_Function("contains", jack.translate(list), new List<Expression>
                                        {
                                            jack.translate(item)
                                        }),
                                        new Literal(false)
                                })
                            })
                        , new List<Expression>
                                {
                                    
                                })
                    });
            }
        }
    }
}
