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
        public string external_name;
        public Region region;
        public Dictionary<string, Dungeon> dungeons = new Dictionary<string, Dungeon>();

        public Realm(Region region)
        {
            name = region.name;
            external_name = region.external_name;
            this.region = region;
        }
    }
}
