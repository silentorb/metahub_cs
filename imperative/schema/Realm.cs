using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.logic.schema;

namespace metahub.imperative.schema
{
    public class Realm
    {
        public string name;
        public Region region;
        public Dictionary<string, Dungeon> dungeons = new Dictionary<string, Dungeon>();

        public Realm(Region region)
        {
            name = region.name;
            this.region = region;
        }
    }
}
