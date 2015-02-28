using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace imperative.schema
{
    public class Realm
    {
        public string name;
        public string external_name;
        public Dictionary<string, Dungeon> dungeons = new Dictionary<string, Dungeon>();
        public Overlord overlord;
        public Dictionary<string, Dungeon_Additional> trellis_additional = new Dictionary<string, Dungeon_Additional>();
        public bool is_external;
        public string class_export = "";

        //public Realm(Namespace region, Overlord overlord)
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

        public void load_additional(Region_Additional additional)
        {
            if (additional.is_external.HasValue)
                is_external = additional.is_external.Value;

            if (additional.space != null)
                external_name = additional.space;

            if (additional.class_export != null)
                class_export = additional.class_export;

            if (additional.trellises != null)
            {
                foreach (var item in additional.trellises)
                {
                    trellis_additional[item.Key] = item.Value;
                }
            }
        }
    }
}
