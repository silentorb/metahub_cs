using System.Collections.Generic;
using System.Linq;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.imperative.schema
{
    public class Portal
    {
        public Kind type;
        public Tie tie;
        public Dungeon dungeon;
        public Rail rail;
        public Rail other_rail;
        public Dungeon other_dungeon;
        public string name;
        public bool is_value = false;

        public Portal(Tie tie, Dungeon dungeon)
        {
            this.tie = tie;
            this.dungeon = dungeon;
            type = tie.type;
            rail = tie.rail;
            other_rail = tie.other_rail;
            name = tie.name;
            is_value = tie.is_value;
        }

        public Portal(string name, Kind type, Dungeon dungeon)
        {
            this.name = name;
            this.dungeon = dungeon;
            this.type = type;
        }

        public void customize_initialize(Block block)
        {
            foreach (Range_Float range in tie.ranges)
            {
                var reference = create_reference(range.path.Count > 0
                                                     ? new Path(range.path.Select((t) => new Tie_Expression(t)))
                                                     : null
                    );
                block.add(new Assignment(reference, "=", new Function_Call("rand",
                    new Expression[]
	                {
	                    new Literal(range.min,
	                                new Signature {type = Kind.Float}),
                                    new Literal(range.max, new Signature {type = Kind.Float})
	                }, true)));
            }

            if (tie.has_setter())
            {
                block.add("post", new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
                    {
                        new Tie_Expression(tie)
                    } ));
            }
        }

        public Tie_Expression create_reference(Expression child = null)
        {
            return new Tie_Expression(tie, child);
        }

        public Signature get_signature()
        {
            return new Signature
            {
                type = type,
                rail = other_rail
            };
        }

        public Profession get_profession()
        {
            return new Profession(type, other_dungeon);
        }

        public object get_default_value()
        {
            if (tie != null)
            {
                if (tie.other_rail != null && tie.other_rail.default_value != null)
                    return tie.other_rail.default_value;

                return tie.property.get_default();
            }

            switch (type)
            {
                case Kind.Int:
                    return 0;

                case Kind.Float:
                    return 0;

                case Kind.String:
                    return "";

                case Kind.Bool:
                    return false;

                default:
                    return null;
            }
        }
    }
}