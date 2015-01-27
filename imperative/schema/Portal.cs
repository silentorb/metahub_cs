using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.imperative.schema
{
    public class Portal
    {
        public Tie tie;
        public Dungeon dungeon;
        public Rail rail;
        public Rail other_rail;
        public string name;
        public bool is_value = false;
        public Portal other_portal;
        public Portal parent;
        public Profession profession;

        public Kind type
        {
            get { return profession.type; }
            set { profession.type = value; }
        }

        public Dungeon other_dungeon
        {
            get { return profession.dungeon; }
            set { profession.dungeon = value; }
        }

        public bool is_list
        {
            get { return profession.is_list; }
        }

        public string fullname
        {
            get { return dungeon.name + "." + name; }
        }

        public Portal(Tie tie, Dungeon dungeon)
        {
            this.tie = tie;
            this.dungeon = dungeon;
            profession = new Profession(tie.type);
            rail = tie.rail;
            other_rail = tie.other_rail;
            name = tie.name;

            if (tie.other_rail != null)
                profession.dungeon = dungeon.overlord.get_dungeon(other_rail);

            if (tie.other_tie != null)
            {
                var d = dungeon.overlord.get_dungeon(tie.other_rail);
                if (d != null)
                    other_portal = dungeon.overlord.get_portal(tie.other_tie);
            }

            is_value = tie.is_value;

        }

        public Portal(string name, Kind type, Dungeon dungeon, Dungeon other_dungeon = null)
        {
            if (type == Kind.reference && other_dungeon == null)
                throw new Exception("Invalid portal.");

            this.name = name;
            this.dungeon = dungeon;
            profession = new Profession(type, other_dungeon);
        }

        public Portal(string name, Profession profession, Dungeon dungeon = null)
        {
            this.name = name;
            this.profession = profession;
            this.dungeon = dungeon;
        }

        public Portal(Portal original, Dungeon new_dungeon)
        {
            tie = original.tie;
            dungeon = new_dungeon;
            profession = new Profession(original.type, original.other_dungeon);
            rail = original.rail;
            other_rail = original.other_rail;
            name = original.name;
            is_value = original.is_value;
            other_portal = original.other_portal;
            parent = original;
        }

        public void customize_initialize(Block block)
        {
            if (tie == null)
                return;

            foreach (Range_Float range in tie.ranges)
            {
                var reference = create_reference(range.path.Count > 0
                                                     ? new Path(range.path.Select((t) => new Tie_Expression(t)))
                                                     : null
                    );
                block.add(new Assignment(reference, "=", new Platform_Function("rand", null,
                    new Expression[]
	                {
	                    new Literal(range.min),
                                    new Literal(range.max)
	                })));
            }

            if (tie.has_setter())
            {
                block.add("post", new Property_Function_Call(Property_Function_Type.set, tie, new List<Expression>
                    {
                        new Tie_Expression(tie)
                    }));
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
            return profession;
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