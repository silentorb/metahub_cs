using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace metahub.imperative.schema
{
    public class Realm
    {
        public string name;
        public string external_name;
        public Dictionary<string, Dungeon> dungeons = new Dictionary<string, Dungeon>();
        public Overlord overlord;

        //public Realm(Region region, Overlord overlord)
        //{
        //    name = region.name;
        //    this.overlord = overlord;
        //    external_name = region.external_name;
        //    this.region = region;
        //}

        public Realm(string name, Overlord overlord)
        {
            this.name = name;
            this.overlord = overlord;
        }

        public Dungeon create_dungeon(string name)
        {
            var dungeon = new Dungeon(name, overlord, this);
            //dungeons[name] = dungeon;
            //overlord.dungeons.Add(dungeon);
            
            return dungeon;
        }
    }
}
