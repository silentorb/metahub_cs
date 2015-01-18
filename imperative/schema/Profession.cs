using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.schema;

namespace metahub.imperative.schema
{
    public class Profession
    {
        public Kind type;
        public bool is_list = false;
        public Dungeon dungeon;

        public Profession(Kind type, Dungeon dungeon = null)
        {
            this.type = type;
            this.dungeon = dungeon;
            is_list = type == Kind.list;
        }

        public Profession(metahub.logic.schema.Signature signature, Overlord overlord)
        {
            type = signature.type;
            is_list = type == Kind.list;
            dungeon = signature.rail != null
                ? overlord.get_dungeon(signature.rail)
                : null;
        }

        public Profession clone()
        {
            return new Profession(type, dungeon) { is_list = is_list };
        }
    }
}
