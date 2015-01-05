using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.imperative.code
{
   static class Piece_Maker
    {
       public static void add_functions(Imp imp, Region region)
       {
           var dungeon = imp.get_dungeon(region.rails["Piece_Maker"]);
           var conflicts = dungeon.rail.get_tie_or_error("conflicts");
           var update_function = dungeon.add_function("update", new List<Parameter>());
           var if_scope = new Scope(update_function.scope);
           var conflict = if_scope.create_symbol("conflict", new Signature(Kind.reference, conflicts.other_rail));

           update_function.expressions = new List<Expression>
            {
                Imp.If(
                Imp.operation(">", new Tie_Expression(conflicts, new Function_Call("count", null, true)), 
                new Literal(0)), new List<Expression>
                {
                    new Declare_Variable(conflict, new Tie_Expression(conflicts, 
                        new Function_Call("last", null, true))),

                new Tie_Expression(conflicts, new Function_Call("pop", null, true))

                })
            };
       }
    }
}
