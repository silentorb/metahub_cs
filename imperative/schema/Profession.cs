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
        public Dungeon dungeon;

        public Profession(Kind type, Dungeon dungeon = null)
        {
            this.type = type;
            this.dungeon = dungeon;
        }

        public Profession(metahub.logic.schema.Signature signature, Overlord overlord)
        {
            type = signature.type;
            dungeon = signature.rail != null
                ? overlord.get_dungeon(signature.rail)
                : null;
        }
    }
}
